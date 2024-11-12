using OpenTK.Graphics.OpenGL;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Numerics;

namespace OngekiFumenEditor.Kernel.Graphics.Base
{
	[Serializable]
	public class Texture : IDisposable
	{
		protected int? _id;
		private Vector2 _textureSize;
		public string Name { get; init; }

		public int ID => _id ?? throw new ArgumentNullException(nameof(_id));
		public int Width => (int)_textureSize.X;
		public int Height => (int)_textureSize.Y;

		public TextureWrapMode TextureWrapS
		{
			get
			{
				int m = 0;
				GL.GetTextureParameteri(ID, GetTextureParameter.TextureWrapS, ref m);
				return (TextureWrapMode)m;
			}
			set
			{
				GL.TextureParameteri(ID, TextureParameterName.TextureWrapS, (int)value);
			}
		}

		public TextureWrapMode TextureWrapT
		{
			get
			{
				int m = 0;
				GL.GetTextureParameteri(ID, GetTextureParameter.TextureWrapT, ref m);
				return (TextureWrapMode)m;
			}
			set
			{
				GL.TextureParameteri(ID, TextureParameterName.TextureWrapT, (int)value);
			}
		}

		public TextureMinFilter TextureMinFilter
		{
			get
			{
				int m = 0;
				GL.GetTextureParameteri(ID, GetTextureParameter.TextureMinFilter, ref m);
				return (TextureMinFilter)m;
			}
			set
			{
				GL.TextureParameteri(ID, TextureParameterName.TextureMinFilter, (int)value);
			}
		}

		public TextureMagFilter TextureMagFilter
		{
			get
			{
				int m = 0;
				GL.GetTextureParameteri(ID, GetTextureParameter.TextureMagFilter, ref m);
				return (TextureMagFilter)m;
			}
			set
			{
				GL.TextureParameteri(ID, TextureParameterName.TextureMagFilter, (int)value);
			}
		}

		public Texture(string name = "Texture")
		{
			Name = name;
		}

		public Texture(Bitmap bmp, string name = "Texture") : this(name)
		{
			int id = 0;
			GL.GenTextures(1, ref id);
			_id = id;

			GL.BindTexture(TextureTarget.Texture2d, ID);

			TextureMinFilter = TextureMinFilter.Linear;
			TextureMagFilter = TextureMagFilter.Linear;
			TextureWrapS = TextureWrapMode.ClampToEdge;
			TextureWrapT = TextureWrapMode.ClampToEdge;

			_textureSize = new Vector2(bmp.Width, bmp.Height);

			var bmp_data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

			GL.TexImage2D(TextureTarget.Texture2d, 0, InternalFormat.Rgba, bmp_data.Width, bmp_data.Height, 0,
				OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bmp_data.Scan0);

			bmp.UnlockBits(bmp_data);
		}

		public override string ToString()
		{
			return $"({ID}){Name}";
		}

		public void Dispose()
		{
			if (_id is int id)
			{
				GL.DeleteTexture(id);
				_id = null;
			}
		}
	}
}