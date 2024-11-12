﻿using OngekiFumenEditor.Utils;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace OngekiFumenEditor.Kernel.Graphics.Base
{
	public class Shader : IDisposable
	{
		private int vertexShader, fragmentShader, program = -1;

		private bool compiled = false;

		private string vert;

		private string frag;

		public string VertexProgram { get { return vert; } set { vert = value; } }

		public string FragmentProgram { get { return frag; } set { frag = value; } }


		public string GeometryProgram { get { return geo; } set { geo = value; } }

		private Dictionary<string, object> _uniforms;

		public Dictionary<string, object> Uniforms { get { return _uniforms; } internal set { _uniforms = value; } }

		private string vertError;
		private string fragError;
		private string geoError;
		public string Error => GenErrorString();

		private string GenErrorString()
		{
			string gen(string name, string msg)
			{
				if (string.IsNullOrWhiteSpace(msg))
					return string.Empty;

				return $"{msg} has compile error(s):{msg}\n";
			}

			return gen(nameof(VertexProgram), vertError) + gen(nameof(FragmentProgram), fragError) + gen(nameof(GeometryProgram), geoError);
		}

		public void Compile()
		{
			if (compiled == false)
			{
				Dispose();

				Uniforms = new Dictionary<string, object>();

				var genShaders = new List<int>();

				void compileShader(string source, ShaderType shaderType, ref int shader, ref string msg)
				{
					shader = GL.CreateShader(shaderType);
					GL.ShaderSource(shader, source);

					GL.CompileShader(shader);
					if (!GL.IsShader(shader))
						throw new Exception($"{shaderType} compile failed.");
					GL.GetShaderInfoLog(shader, out msg);
					if (!string.IsNullOrEmpty(msg))
						Log.LogDebug($"[{shaderType}]:{msg}");

					genShaders.Add(shader);
				}

				compileShader(VertexProgram, ShaderType.VertexShader, ref vertexShader, ref vertError);
				compileShader(FragmentProgram, ShaderType.FragmentShader, ref fragmentShader, ref fragError);
				if (!string.IsNullOrWhiteSpace(GeometryProgram))
					compileShader(GeometryProgram, ShaderType.GeometryShader, ref geometryShader, ref geoError);

				program = GL.CreateProgram();

				foreach (var shader in genShaders)
					GL.AttachShader(program, shader);

				GL.LinkProgram(program);

				GL.GetProgramInfoLog(program, out var buildShaderError);
				if (!string.IsNullOrEmpty(buildShaderError))
					Log.LogError(buildShaderError);

				GL.GetProgrami(program, ProgramProperty.ActiveUniforms, out var total);

				for (int i = 0; i < total; i++)
					GL.GetActiveUniform(program, (uint)i, 16, out _, out _, out _, out var _);

				compiled = true;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Begin()
		{
			GL.UseProgram(program);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void End()
		{
			GL.UseProgram(0);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void PassUniform(string name, Texture tex)
		{
			if (tex == null)
			{
				PassNullTexUniform(name);
				return;
			}

			GL.BindTexture(TextureTarget.Texture2d, tex.ID);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void PassNullTexUniform(string name)
		{
			GL.BindTexture(TextureTarget.Texture2d, 0);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void PassUniform(int l, float v) => GL.Uniform1f(l, v);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void PassUniform(string name, float val)
		{
			int l = GetUniformLocation(name);
			PassUniform(l, val);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void PassUniform(int l, int v) => GL.Uniform1f(l, v);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void PassUniform(string name, int val)
		{
			int l = GetUniformLocation(name);
			PassUniform(l, val);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void PassUniform(int l, Vector2 v) => GL.Uniform2f(l, 1, ref v);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void PassUniform(string name, Vector2 val)
		{
			int l = GetUniformLocation(name);
			PassUniform(l, val);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void PassUniform(int l, Vector4 v) => GL.Uniform4f(l, 1, ref v);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void PassUniform(string name, Vector4 val)
		{
			int l = GetUniformLocation(name);
			PassUniform(l, val);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void PassUniform(int l, System.Numerics.Vector2 v) => PassUniform(l, new Vector2(v.X, v.Y));
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void PassUniform(string name, System.Numerics.Vector2 val)
		{
			int l = GetUniformLocation(name);
			PassUniform(l, val);
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void PassUniform(int l, Matrix4 v) => GL.UniformMatrix4f(l, 1, false, ref v);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void PassUniform(string name, Matrix4 matrix4)
		{
			int l = GetUniformLocation(name);
			PassUniform(l, matrix4);
		}

		private Dictionary<string, int> _uniformDictionary = new Dictionary<string, int>();
		private Dictionary<string, int> _attrbDictionary = new Dictionary<string, int>();
		private string geo;
		private int geometryShader;

		public int GetUniformLocation(string name)
		{
			if (!_uniformDictionary.TryGetValue(name, out var l))
			{
				l = GL.GetUniformLocation(program, name);
				_uniformDictionary[name] = l;
			}

			return l;
		}

		public int GetAttribLocation(string name)
		{
			if (!_attrbDictionary.TryGetValue(name, out var l))
			{
				l = GL.GetAttribLocation(program, name);
				_attrbDictionary[name] = l;
			}

			return l;
		}

		public void Dispose()
		{
			if (program < 0)
				return;
			GL.DeleteShader(vertexShader);
			GL.DeleteShader(fragmentShader);
			GL.DeleteProgram(program);
			program = -1;
		}

		public int ShaderProgram => program;
	}
}