using OngekiFumenEditor.Utils;
using OpenTK.Graphics.OpenGL;
using System;
using System.Drawing;

namespace OngekiFumenEditor.Kernel.Graphics.Drawing.DefaultDrawingImpl.StringDrawing.String
{
	public unsafe class Texture : IDisposable
	{
		private readonly int _handle;

		public readonly int Width;
		public readonly int Height;

		public Texture(int width, int height)
		{
			Width = width;
			Height = height;

			_handle = GL.GenTexture();
			GLUtility.CheckError();
			Bind();

			//Reserve enough memory from the gpu for the whole image
			GL.TexImage2D(TextureTarget.Texture2d, 0, InternalFormat.Rgba8, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
			GLUtility.CheckError();

			SetParameters();
		}

		private void SetParameters()
		{
			//Setting some texture perameters so the texture behaves as expected.
			GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
			GLUtility.CheckError();

			GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
			GLUtility.CheckError();

			GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
			GLUtility.CheckError();

			GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMagFilter, (int)TextureMinFilter.Linear);
			GLUtility.CheckError();

			//Generating mipmaps.
			GL.GenerateMipmap(TextureTarget.Texture2d);
			GLUtility.CheckError();
		}

		public void Bind(TextureUnit textureSlot = TextureUnit.Texture0)
		{
			//When we bind a texture we can choose which textureslot we can bind it to.
			GL.ActiveTexture(textureSlot);
			GLUtility.CheckError();

			GL.BindTexture(TextureTarget.Texture2d, _handle);
			GLUtility.CheckError();
		}

		public void Dispose()
		{
			//In order to dispose we need to delete the opengl handle for the texure.
			GL.DeleteTexture(_handle);
			GLUtility.CheckError();
		}

		public void SetData(Rectangle bounds, byte[] data)
		{
			Bind();
			fixed (byte* ptr = data)
			{
				GL.TexSubImage2D(
					target: TextureTarget.Texture2d,
					level: 0,
					xoffset: bounds.Left,
					yoffset: bounds.Top,
					width: bounds.Width,
					height: bounds.Height,
					format: PixelFormat.Rgba,
					type: PixelType.UnsignedByte,
					pixels: new IntPtr(ptr)
				);
				GLUtility.CheckError();
			}
		}
	}
}