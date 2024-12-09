using OngekiFumenEditor.Kernel.Graphics;
using OngekiFumenEditor.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace OngekiFumenEditor.Modules.FumenVisualEditor.Views.UI
{
	/// <summary>
	/// Toast.xaml 的交互逻辑
	/// </summary>
	public partial class Toast : UserControl
	{
		private WindowOverlay overlay;
		public void ShowMessage(string message, MessageType message_type = MessageType.Notify, uint show_time = 2000) => InternalShowMessage(message, message_type, show_time);

		public enum MessageType
		{
			Error,
			Warn,
			Notify
		}

		private Dictionary<MessageType, Vortice.Mathematics.Color> TextColors = new()
		{
			{MessageType.Error,new Vortice.Mathematics.Color(0xFF,0x00,0x00) },
			{MessageType.Warn,new Vortice.Mathematics.Color(0xFF,0xFF,0xE0) },
			{MessageType.Notify,new Vortice.Mathematics.Color(0xFF,0xFF,0xFF) },
		};

		public Toast()
		{
			InitializeComponent();
			Loaded += Toast_Loaded;
		}

		private void Toast_Loaded(object sender, RoutedEventArgs e)
		{
			Loaded -= Toast_Loaded;
			overlay = WindowOverlay.Manager.GetInst(this);
		}

		private void InternalShowMessage(string message, MessageType message_type = MessageType.Notify, uint show_time = 2000)
		{
			_ = overlay.DrawToastAsync(message, new(0x1D, 0x20, 0x31, 0xBD), TextColors[message_type]);
			Log.LogDebug($"{message_type} {message}");
		}
	}
}
