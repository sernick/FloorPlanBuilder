using System.Windows;
using System.Windows.Input;

using MahApps.Metro.Controls;


namespace FloorPlanBuilder.Views
{
	public partial class OriginControl
	{
		#region Constructors

		public OriginControl()
		{
			InitializeComponent();
		}

		#endregion

		#region Methods

		private void NumericUpDown_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			var control = sender as NumericUpDown;
			if (control != null)
			{
				if (!control.IsKeyboardFocusWithin)
				{
					e.Handled = true;
					control.Focus();
				}
			}
		}

		private void NumericUpDown_SelectAll(object sender, RoutedEventArgs e)
		{
			var control = sender as NumericUpDown;
			control?.SelectAll();
		}

		#endregion
	}
}