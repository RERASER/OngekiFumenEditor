using FontStashSharp;
using OngekiFumenEditor.Kernel.Graphics.Drawing.DefaultDrawingImpl.StringDrawing.String.Platform;
using OngekiFumenEditor.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Numerics;

namespace OngekiFumenEditor.Kernel.Graphics.Drawing.DefaultDrawingImpl.StringDrawing
{
	[Export(typeof(IStringDrawing))]
	[PartCreationPolicy(CreationPolicy.Shared)]
	public class DefaultStringDrawing : CommonDrawingBase, IStringDrawing, IDisposable
	{
		private class FontHandle : IStringDrawing.FontHandle
		{
			public string Name { get; set; }
			public string FilePath { get; set; }
		}

		public static IEnumerable<IStringDrawing.FontHandle> DefaultSupportFonts { get; } = GetSupportFonts();
		public static IStringDrawing.FontHandle DefaultFont { get; } = GetSupportFonts().FirstOrDefault(x => x.Name.ToLower() == "consola");

		public IEnumerable<IStringDrawing.FontHandle> SupportFonts { get; } = DefaultSupportFonts;

		private static IEnumerable<IStringDrawing.FontHandle> GetSupportFonts()
		{
			return Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.Fonts)).Select(x => new FontHandle
			{
				Name = Path.GetFileNameWithoutExtension(x),
				FilePath = x
			}).Where(x => Path.GetExtension(x.FilePath).ToLower() == ".ttf").ToArray();
		}

		public DefaultStringDrawing()
		{
		}

		public void Draw(string text, Vector2 pos, Vector2 scale, int fontSize, float rotate, Vector4 color, Vector2 origin, IStringDrawing.StringStyle style, IDrawingContext target, IStringDrawing.FontHandle handle, out Vector2? measureTextSize)
		{
			target.PerfomenceMonitor.OnBeginDrawing(this);

			measureTextSize = OpenTK.Wpf.DWriteCore.Measure(text, fontSize, (int)style);
			OpenTK.Wpf.DWriteCore.Draw(text, pos, fontSize, color, origin,target.Rect,(int)style);
			target.PerfomenceMonitor.OnAfterDrawing(this);
		}

		public void Dispose()
		{
		}
	}
}
