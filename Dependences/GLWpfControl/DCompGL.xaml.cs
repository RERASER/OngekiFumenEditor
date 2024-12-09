using OpenTK.Wpf.Interop;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;

namespace OpenTK.Wpf
{
	/// <summary>
	/// DCompGL.xaml 的交互逻辑
	/// </summary>
	public partial class DCompGL : UserControl,IDisposable
	{
		DCompGLHost host;
		GLWpfControl GLCore;
		Thread LoopThread;
		DpiScale currentDpi;
		IntPtr hwndHost;
		int hostWidth;
		int hostHeight;

		public event Action? Ready { add { GLCore.Ready += value; } remove { GLCore.Ready -= value; } }
		public event Action<TimeSpan>? Render { add { GLCore.Render += value; } remove { GLCore.Render -= value; } }
		public new event System.Windows.Input.MouseWheelEventHandler MouseWheel { add { CompositionHostElement.MouseWheel += value; } remove { CompositionHostElement.MouseWheel -= value; } }
		public new event System.Windows.Input.MouseButtonEventHandler PreviewMouseDown { add { CompositionHostElement.PreviewMouseDown += value; } remove { CompositionHostElement.PreviewMouseDown -= value; } }
		public new event System.Windows.Input.MouseButtonEventHandler PreviewMouseUp { add { CompositionHostElement.PreviewMouseUp += value; } remove { CompositionHostElement.PreviewMouseUp -= value; } }
		public new event System.Windows.Input.MouseEventHandler MouseMove { add { CompositionHostElement.MouseMove += value; } remove { CompositionHostElement.MouseMove -= value; } }
		public new event System.Windows.Input.MouseEventHandler MouseLeave { add { CompositionHostElement.MouseLeave += value; } remove { CompositionHostElement.MouseLeave -= value; } }
		volatile bool loop = true;
		private double hostWidthWithDPI;
		private double hostHeightWithDPI;
		private bool disposedValue;

		[Flags]
		enum WindowStyle : int
		{
			WS_CLIPCHILDREN = 0x02000000,
			WS_CHILD = 0x40000000,
			WS_VISIBLE = 0x10000000,
			LBS_NOTIFY = 0x00000001,
			HOST_ID = 0x00000002,
			LISTBOX_ID = 0x00000001,
			WS_VSCROLL = 0x00200000,
			WS_BORDER = 0x00800000,
		}

		[Flags]
		enum WindowStyleEx : int
		{
			WS_EX_NOREDIRECTIONBITMAP = 0x00200000,
		}

		public DCompGL()
		{
			InitializeComponent();
			hwndHost = CreateWindowEx(
			(int)(WindowStyleEx.WS_EX_NOREDIRECTIONBITMAP), "STATIC", "",
			(int)(WindowStyle.WS_CHILD | WindowStyle.WS_VISIBLE),
			0, 0,
			1, 1,
			GetDesktopWindow(),
			(int)WindowStyle.HOST_ID,
			IntPtr.Zero, 0);
			ShowWindow(hwndHost, 0);
			host = new(hwndHost);
			CompositionHostElement.Child = host;
			Loaded += DCompGL_Loaded;
			GLCore = new();
			LoopThread = new(EntryPoint);
			LoopThread.Start();
		}

		protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
		{
			hostWidthWithDPI = RenderSize.Width;
			hostHeightWithDPI = RenderSize.Height;
			hostWidth = (int)(hostWidthWithDPI * currentDpi.DpiScaleX);
			hostHeight = (int)(hostHeightWithDPI * currentDpi.DpiScaleY);
			SetWindowPos(hwndHost, 0, 0, 0, (int)hostWidth, (int)hostHeight, 0x4000 | 0x0200 | 0x0008 | 0x0002);
			base.OnRenderSizeChanged(sizeInfo);
		}

		private void DCompGL_Loaded(object sender, RoutedEventArgs e)
		{
			loop = true;
			hostWidthWithDPI = RenderSize.Width;
			hostHeightWithDPI = RenderSize.Height;
			double dpi = DXInterop.GetDpiForSystem() / 96d;
			currentDpi = new DpiScale(dpi, dpi);
			hostWidth = (int)(hostWidthWithDPI * currentDpi.DpiScaleX);
			hostHeight = (int)(hostHeightWithDPI * currentDpi.DpiScaleY);
			SetWindowPos(hwndHost, 0, 0, 0, (int)hostWidth, (int)hostHeight, 0x4000 | 0x0200 | 0x0008 | 0x0002);
		}

		public void EntryPoint()
		{
			var DesignMode = false;
			Dispatcher.Invoke(() =>
			{
				DesignMode = DesignerProperties.GetIsInDesignMode(this);
			});
			while (loop)
			{
				if (hwndHost != IntPtr.Zero)
				{
					Dispatcher.Invoke(() =>
					{	
						GLCore.OnRender(DesignMode, hostWidthWithDPI, hostHeightWithDPI);
					});
					GLCore.RenderD3D();
					GLCore.WaitForVBlank();
				}
				else
				{
					Thread.Sleep(32);
				}
			}
			Dispatcher.Invoke(() =>
			{
				host?.Dispose();
				GLCore?.Dispose();
				DestroyWindow(hwndHost);
			});
		}

		public void Start(GLWpfControlSettings settings)
		{
			Dispatcher.Invoke(() =>
			{
				GLCore.Start(settings,hwndHost);
			});
		}

		[DllImport("user32.dll", EntryPoint = "CreateWindowEx", CharSet = CharSet.Unicode)]
		private static extern IntPtr CreateWindowEx(int dwExStyle,
													  string lpszClassName,
													  string lpszWindowName,
													  int style,
													  int x, int y,
													  int width, int height,
													  IntPtr hwndParent,
													  IntPtr hMenu,
													  IntPtr hInst,
													  IntPtr pvParam);

		[DllImport("user32.dll", CharSet = CharSet.Unicode)]
		private static extern bool DestroyWindow(IntPtr hwnd);

		[DllImport("user32.dll", CharSet = CharSet.Unicode)]
		private static extern bool ShowWindowAsync(IntPtr hwnd, int nCmdShow);

		[DllImport("user32.dll", CharSet = CharSet.Unicode)]
		private static extern bool ShowWindow(IntPtr hwnd, int nCmdShow);

		[DllImport("user32.dll", CharSet = CharSet.Unicode)]
		private static extern IntPtr GetDesktopWindow();

		[DllImport("user32.dll")]
		private static extern bool SetWindowPos(IntPtr hwnd, IntPtr hwndInsertAfter, int X, int Y, int cx, int cy, uint flag);

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{

				}
				loop = false;
				disposedValue = true;
			}
		}

		// // TODO: 仅当“Dispose(bool disposing)”拥有用于释放未托管资源的代码时才替代终结器
		~DCompGL()
		{
			// 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
			Dispose(disposing: false);
		}

		public void Dispose()
		{
			// 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}

	public partial class DCompGLHost : HwndHost
	{
		private IntPtr hwndHost;
		public DCompGLHost(IntPtr hwnd)
		{
			hwndHost = hwnd;
		}
		protected override HandleRef BuildWindowCore(HandleRef hwndParent)
		{
			var test = SetParent(hwndHost, hwndParent.Handle);
			ShowWindow(hwndHost, 5);
			return new HandleRef(this, hwndHost);
		}

		protected override nint WndProc(nint hwnd, int msg, nint wParam, nint lParam, ref bool handled)
		{
			switch (msg)
			{
				case 0x0005:
					//var width = LOWORD((uint)lParam);
					//var height = HIWORD((uint)lParam);
					handled = true;
					return 0;
				case 0x000F:
					handled = true;
					return 0;
				default:
					break;
			}
			return base.WndProc(hwnd, msg, wParam, lParam, ref handled);
		}

		private static ushort LOWORD(uint value)
		{
			return (ushort)(value & 0xFFFF);
		}

		private static ushort HIWORD(uint value)
		{
			return (ushort)(value >> 16);
		}

		protected override void DestroyWindowCore(HandleRef hwnd)
		{
			ShowWindow(hwndHost, 0);
		}
		
		[DllImport("user32.dll")]
		private static extern IntPtr SetParent(IntPtr hwnd, IntPtr hWndParent);

		[DllImport("user32.dll", CharSet = CharSet.Unicode)]
		private static extern bool ShowWindowAsync(IntPtr hwnd, int nCmdShow);

		[DllImport("user32.dll", CharSet = CharSet.Unicode)]
		private static extern bool ShowWindow(IntPtr hwnd, int nCmdShow);
	}
}
