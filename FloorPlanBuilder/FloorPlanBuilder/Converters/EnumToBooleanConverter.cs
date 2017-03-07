﻿using System;
using System.Globalization;
using System.Windows.Data;


namespace FloorPlanBuilder.Converters
{
	public class EnumToBooleanConverter : IValueConverter
	{
		#region Methods

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value != null && value.Equals(parameter);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value.Equals(true) ? parameter : Binding.DoNothing;
		}

		#endregion
	}
}