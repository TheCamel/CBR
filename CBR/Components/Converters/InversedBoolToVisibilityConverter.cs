using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;

namespace CBR.Components.Converters
{
	[ValueConversion(typeof(bool), typeof(Visibility))]
	public class InversedBoolToVisibilityConverter : IValueConverter
	{
		/// <summary>
		/// Singleton access
		/// </summary>
		public static readonly InversedBoolToVisibilityConverter Instance = new InversedBoolToVisibilityConverter(); 

		/// <summary>
		/// Private constructor for singleton pattern
		/// </summary>
		private InversedBoolToVisibilityConverter()
		{
		}

		/// <summary>
		/// IValueConverter.Convert
		/// </summary>
		/// <param name="value"></param>
		/// <param name="targetType"></param>
		/// <param name="parameter"></param>
		/// <param name="culture"></param>
		/// <returns></returns>
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (value is bool && targetType == typeof(Visibility))
			{
				bool val = !(bool)value;

				if (val)
					return Visibility.Visible;
				else
					if (parameter != null && parameter is Visibility)
						return parameter;
					else
						return Visibility.Collapsed; 
			}

			return Visibility.Visible;
		}

		/// <summary>
		/// IValueConverter.ConvertBack
		/// </summary>
		/// <param name="value"></param>
		/// <param name="targetType"></param>
		/// <param name="parameter"></param>
		/// <param name="culture"></param>
		/// <returns></returns>
		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (value is Visibility && targetType == typeof(bool))
			{
				Visibility val = (Visibility)value;
				if (val == Visibility.Visible)
					return false;
				else
					return true;
			}
			throw new ArgumentException("Invalid argument/return type. Expected argument: Visibility and return type: bool"); 
		}
	}
}
