using OngekiFumenEditor.Utils;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System;
using System.Numerics;

namespace OngekiFumenEditor.Kernel.Graphics.Drawing.DefaultDrawingImpl.StringDrawing.String
{
	public class Shader : IDisposable
	{
		private int _handle;

		public Shader(string vertexContent, string fragmentContent)
		{
			int vertex = LoadShader(ShaderType.VertexShader, vertexContent);
			int fragment = LoadShader(ShaderType.FragmentShader, fragmentContent);
			_handle = GL.CreateProgram();
			GLUtility.CheckError();

			GL.AttachShader(_handle, vertex);
			GLUtility.CheckError();

			GL.AttachShader(_handle, fragment);
			GLUtility.CheckError();

			GL.LinkProgram(_handle);
			GL.GetProgrami(_handle, ProgramProperty.LinkStatus, out var status);
			GL.GetProgramInfoLog(_handle, out var infoLog);
			if (status == 0)
			{
				throw new Exception($"Program failed to link with error: {infoLog}");
			}

			GL.DetachShader(_handle, vertex);
			GL.DetachShader(_handle, fragment);

			GL.DeleteShader(vertex);
			GL.DeleteShader(fragment);
		}

		public void Use()
		{
			GL.UseProgram(_handle);
			GLUtility.CheckError();
		}

		public void SetUniform(string name, int value)
		{
			int location = GL.GetUniformLocation(_handle, name);
			if (location == -1)
			{
				throw new Exception($"{name} uniform not found on shader.");
			}
			GL.Uniform1i(location, value);
			GLUtility.CheckError();
		}

		public void SetUniform(string name, float value)
		{
			int location = GL.GetUniformLocation(_handle, name);
			if (location == -1)
			{
				throw new Exception($"{name} uniform not found on shader.");
			}
			GL.Uniform1f(location, value);
			GLUtility.CheckError();
		}

		public void SetUniform(string name, Matrix4x4 value)
		{
			int location = GL.GetUniformLocation(_handle, name);
			if (location == -1)
			{
				throw new Exception($"{name} uniform not found on shader.");
			}

			GL.UniformMatrix4f(location, 1, false, ref value);
			GLUtility.CheckError();
		}

		public void SetUniform(string name, Matrix4 value)
		{
			int location = GL.GetUniformLocation(_handle, name);
			if (location == -1)
			{
				throw new Exception($"{name} uniform not found on shader.");
			}
			GL.UniformMatrix4f(location, 1, false, ref value);
			GLUtility.CheckError();
		}

		public void Dispose()
		{
			GL.DeleteProgram(_handle);
		}

		private int LoadShader(ShaderType type, string src)
		{
			int handle = GL.CreateShader(type);
			GLUtility.CheckError();

			GL.ShaderSource(handle, src);
			GLUtility.CheckError();

			GL.CompileShader(handle);
			GL.GetShaderInfoLog(handle, out var infoLog);
			if (!string.IsNullOrWhiteSpace(infoLog))
			{
				throw new Exception($"Error compiling shader of type {type}, failed with error {infoLog}");
			}

			return handle;
		}

		public int GetAttribLocation(string attribName)
		{
			var result = GL.GetAttribLocation(_handle, attribName);
			GLUtility.CheckError();
			return result;
		}
	}
}