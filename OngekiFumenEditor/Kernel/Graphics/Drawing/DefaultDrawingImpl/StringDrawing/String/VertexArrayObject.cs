using OngekiFumenEditor.Utils;
using OpenTK.Graphics.OpenGL;
using System;

namespace OngekiFumenEditor.Kernel.Graphics.Drawing.DefaultDrawingImpl.StringDrawing.String
{
	public class VertexArrayObject : IDisposable
	{
		private readonly int _handle;
		private readonly int _stride;

		public VertexArrayObject(int stride)
		{
			if (stride <= 0)
			{
				throw new ArgumentOutOfRangeException(nameof(stride));
			}

			_stride = stride;

			GL.GenVertexArrays(1, ref _handle);
			GLUtility.CheckError();
		}

		public void Dispose()
		{
			GL.DeleteVertexArray(_handle);
			GLUtility.CheckError();
		}

		public void Bind()
		{
			GL.BindVertexArray(_handle);
			GLUtility.CheckError();
		}

		public unsafe void VertexAttribPointer(int location, int size, VertexAttribPointerType type, bool normalized, int offset)
		{
			GL.EnableVertexAttribArray((uint)location);
			GL.VertexAttribPointer((uint)location, size, type, normalized, _stride, new IntPtr(offset));
			GLUtility.CheckError();
		}
	}
}
