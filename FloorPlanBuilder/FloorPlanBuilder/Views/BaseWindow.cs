using System;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Media;

using MahApps.Metro.Controls;


namespace FloorPlanBuilder.Views
{
	public class BaseWindow : MetroWindow
	{
		#region Constructors

		public BaseWindow()
		{
			ShowIconOnTitleBar = false;
			TitleCharacterCasing = CharacterCasing.Normal;
			UseLayoutRounding = true;

			try
			{
				GlowBrush = FindResource("AccentColorBrush") as Brush;
			}
			catch (Exception)
			{
				//
			}

			try
			{
				var drawing = FindResource("PointLogoForTaskbar") as Drawing;
				if (drawing != null)
				{
					Icon = new DrawingImage(drawing);
				}
			}
			catch (Exception)
			{
				//
			}
		}

		#endregion

		#region Properties

		public bool IsInDesignMode
		{
			get
			{
				Process process = Process.GetCurrentProcess();
				bool result = process.ProcessName == "devenv";
				process.Dispose();
				return result;
			}
		}

		#endregion
	}
}