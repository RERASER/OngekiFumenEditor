using ExCSS;
using OpenTK.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using Vortice.Direct2D1;

namespace OngekiFumenEditor.Kernel.Graphics
{
	internal class WindowOverlay
	{
		public static class Manager
		{
			private static System.Collections.Concurrent.ConcurrentDictionary<nint, WindowOverlay> insts = new();

			public static WindowOverlay GetInst(DependencyObject dependencyObject)
			{
				var window = Window.GetWindow(dependencyObject);
				var wih = new System.Windows.Interop.WindowInteropHelper(window);
				return insts.GetOrAdd(wih.Handle, (hwnd) =>
				{
					WindowOverlay overlay = new(window);
					overlay.InitDirect3D(true);
					window.Closed += (o, e) =>
					{
						overlay.DestoryDirect3D();
						insts.TryRemove(hwnd, out _);
					};
					return overlay;
				});
			}
		}


		private Vortice.Direct3D11.ID3D11Device _device = null;

		private Vortice.Direct3D12.ID3D12Device _device12 = null;
		private Vortice.Direct3D12.ID3D12CommandQueue _commandQueue12 = null;

		private Vortice.DXGI.IDXGIFactory2 _DXGIFactory2 = null;
		private Vortice.DXGI.IDXGIDevice1 _DXGIDevice1 = null;

		private Vortice.DirectComposition.IDCompositionDevice _DcompDevice = null;
		private Vortice.DirectComposition.IDCompositionTarget _DcompTarget = null;
		private Vortice.DirectComposition.IDCompositionVisual _DcompVisual = null;

		private Vortice.Direct2D1.ID2D1Factory1 _factory2D = null;
		private Vortice.Direct2D1.ID2D1Device _device2D = null;

		private Vortice.UIAnimation.IUIAnimationManager2 _UIAnimationManager2 = new();

		private static Vortice.DirectWrite.IDWriteFactory _DWriteFactory = Vortice.DirectWrite.DWrite.DWriteCreateFactory<Vortice.DirectWrite.IDWriteFactory>();

		private DpiScale currentDpi;
		private IntPtr hwndHost;
		private Window window;
		private int hostWidth;
		private int hostHeight;

		private WindowOverlay(System.Windows.Window window)
		{
			this.window = window;
			var wih = new System.Windows.Interop.WindowInteropHelper(window);
			hwndHost = wih.Handle;
			currentDpi = System.Windows.Media.VisualTreeHelper.GetDpi(window);
			hostWidth = (int)(window.ActualWidth * currentDpi.DpiScaleX);
			hostHeight = (int)(window.ActualHeight * currentDpi.DpiScaleY);
		}

		private void InitDirect3D(bool UseDirect3D12)
		{
			if (UseDirect3D12)
			{
				var hr = Vortice.Direct3D12.D3D12.D3D12CreateDevice(null, out _device12);
				_commandQueue12 = _device12.CreateCommandQueue(Vortice.Direct3D12.CommandListType.Direct);
				var hr1 = Vortice.Direct3D11on12.Apis.D3D11On12CreateDevice(_device12, Vortice.Direct3D11.DeviceCreationFlags.BgraSupport, [Vortice.Direct3D.FeatureLevel.Level_11_1], [_commandQueue12], 0, out _device, out _, out _);
			}
			else
			{
				var hr = Vortice.Direct3D11.D3D11.D3D11CreateDevice(null, Vortice.Direct3D.DriverType.Hardware, Vortice.Direct3D11.DeviceCreationFlags.BgraSupport, [Vortice.Direct3D.FeatureLevel.Level_11_1], out _device);
			}
			_DXGIDevice1 = _device.QueryInterface<Vortice.DXGI.IDXGIDevice1>();
			_DXGIDevice1.SetMaximumFrameLatency(1);
			_DXGIFactory2 = Vortice.DXGI.DXGI.CreateDXGIFactory2<Vortice.DXGI.IDXGIFactory2>(true);
			_factory2D = Vortice.Direct2D1.D2D1.D2D1CreateFactory<Vortice.Direct2D1.ID2D1Factory1>(Vortice.Direct2D1.FactoryType.MultiThreaded, Vortice.Direct2D1.DebugLevel.None);
			_device2D = _factory2D.CreateDevice(_DXGIDevice1);
			var hr2 = Vortice.DirectComposition.DComp.DCompositionCreateDevice2(_device2D, out _DcompDevice);
			var hr4 = _DcompDevice.CreateVisual(out _DcompVisual);
			_DcompVisual.SetOffsetX(0);
			_DcompVisual.SetOffsetY(0);
			var hr3 = _DcompDevice.CreateTargetForHwnd(hwndHost, true, out _DcompTarget);
			_DcompTarget.SetRoot(_DcompVisual);
		}

		public async Task DrawToastAsync(string text, Vortice.Mathematics.Color backgroundColor, Vortice.Mathematics.Color textColor)
		{
			hostWidth = (int)(window.ActualWidth * currentDpi.DpiScaleX);
			hostHeight = (int)(window.ActualHeight * currentDpi.DpiScaleY);
			var toastVisual1 = _DcompDevice.CreateVisual();
			var toastVisual = toastVisual1.QueryInterface<Vortice.DirectComposition.IDCompositionVisual3>();
			var format = _DWriteFactory.CreateTextFormat("MSYH", Vortice.DirectWrite.FontWeight.SemiBold, Vortice.DirectWrite.FontStyle.Normal, 16);
			var layout = _DWriteFactory.CreateTextLayout(text, format, float.PositiveInfinity, float.PositiveInfinity);
			var surfaceWidth = (uint)layout.Metrics.Width + 36;
			var realsurfaceWidth = (uint)(surfaceWidth * currentDpi.DpiScaleX);
			var surfaceHeight = (uint)layout.Metrics.Height + 16;
			var realsurfaceHeight = (uint)(surfaceHeight * currentDpi.DpiScaleY);
			_DcompDevice.CreateSurface(realsurfaceWidth, realsurfaceHeight, Vortice.DXGI.Format.R8G8B8A8_UNorm, Vortice.DXGI.AlphaMode.Premultiplied, out var surface);
			var context = surface.BeginDraw<ID2D1DeviceContext>(null, out var updateOffset);
			context.SetDpi((float)currentDpi.PixelsPerInchX, (float)currentDpi.PixelsPerInchY);
			updateOffset = new Vortice.Mathematics.Int2((int)(updateOffset.X / currentDpi.DpiScaleX), (int)(updateOffset.Y / currentDpi.DpiScaleY));
			var backgroundbrush = context.CreateSolidColorBrush(backgroundColor);
			var textbrush = context.CreateSolidColorBrush(textColor);
			context.FillRoundedRectangle(new() { RadiusX = 8, RadiusY = 8, Rect = new(updateOffset.X, updateOffset.Y, surfaceWidth + updateOffset.X, surfaceHeight + updateOffset.Y) }, backgroundbrush);
			context.DrawTextLayout(new(updateOffset.X + 18, updateOffset.Y + 8), layout, textbrush);
			surface.EndDraw();
			toastVisual.SetContent(surface);
			_DcompVisual.AddVisual(toastVisual, false, null);
			toastVisual.SetOffsetX(hostWidth / 2 - surfaceWidth / 2);
			toastVisual.SetOffsetY((hostHeight * 0.6f - surfaceHeight / 2));
			var opanim = _DcompDevice.CreateAnimation();
			var offsetYanim = _DcompDevice.CreateAnimation();
			var offsetZanim = _DcompDevice.CreateAnimation();
			var angleanim = _DcompDevice.CreateAnimation();
			var trans3D = _DcompDevice.CreateTranslateTransform3D();
			var rot3D = _DcompDevice.CreateRotateTransform3D();
			opanim.AddCubic(0, 0, 2, 0, 0);
			opanim.End(0.5, 1);
			offsetYanim.AddCubic(0, surfaceHeight*2, -surfaceHeight * 4, 0, 0);
			offsetYanim.End(0.5, 0);
			offsetZanim.AddCubic(0, surfaceHeight, -surfaceHeight *2, 0, 0);
			offsetZanim.End(0.5, 0);
			angleanim.AddCubic(0, -90, 180, 0, 0);
			angleanim.End(0.5, 0);
			trans3D.SetOffsetY(offsetYanim);
			trans3D.SetOffsetZ(offsetZanim);
			rot3D.SetAxisX(1);
			rot3D.SetAxisY(0);
			rot3D.SetAxisZ(0);
			rot3D.SetAngle(angleanim);
			var transg = _DcompDevice.CreateTransform3DGroup([trans3D, rot3D]);
			toastVisual.SetOpacity(opanim);
			toastVisual.SetTransform(transg);
			toastVisual.SetBitmapInterpolationMode(Vortice.DirectComposition.BitmapInterpolationMode.NearestNeighbor);
			_DcompDevice.Commit();
			transg.Dispose();
			trans3D.Dispose();
			rot3D.Dispose();
			angleanim.Dispose();
			offsetYanim.Dispose();
			offsetZanim.Dispose();
			backgroundbrush.Dispose();
			textbrush.Dispose();
			context.Dispose();
			layout.Dispose();
			format.Dispose();
			await Task.Delay(2000);
			opanim.Reset();
			opanim.AddCubic(0, 1, -5, 0, 0);
			opanim.End(0.2, 0);
			toastVisual.SetOpacity(opanim);
			_DcompDevice.Commit();
			opanim.Dispose();
			await Task.Delay(200);
			_DcompVisual.RemoveVisual(toastVisual);
			toastVisual.Dispose();
			toastVisual1.Dispose();
		}

		private void DestoryDirect3D()
		{
			_DcompTarget?.Dispose();
			_DcompTarget = null;
			_DcompVisual?.Dispose();
			_DcompVisual = null;
			_DcompTarget?.Dispose();
			_DcompTarget = null;
			_DcompDevice?.Dispose();
			_DcompDevice = null;
			_DXGIFactory2?.Dispose();
			_DXGIFactory2 = null;
			_device2D?.Dispose();
			_factory2D?.Dispose();
			_DXGIDevice1?.Dispose();
			_device?.Dispose();
			_commandQueue12?.Dispose();
			_device12?.Dispose();
		}
	}
}
