using System;
using System.Globalization;
using System.Windows.Data;

using FloorPlanBuilder.Classes;


namespace FloorPlanBuilder.Converters
{
	internal class NewItemPlaceholderToNullConverter : IValueConverter
	{
		#region Methods

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value != null && value.ToString() == Singleton.Instance.NewItemPlaceholder)
			{
				value = null;
			}
			return value;
		}

		#endregion
	}
}