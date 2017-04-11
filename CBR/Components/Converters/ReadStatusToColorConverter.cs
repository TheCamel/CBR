using System;
using System.Windows.Data;

namespace CBR.Components.Converters
{
	[ValueConversion(typeof(bool), typeof(System.Windows.Media.SolidColorBrush))]
	public class ReadStatusToColorConverter : IValueConverter
	{
        /// <summary>
		/// Singleton access
		/// </summary>
		public static readonly ReadStatusToColorConverter Instance = new ReadStatusToColorConverter(); 

		/// <summary>
		/// Private constructor for singleton pattern
		/// </summary>
        private ReadStatusToColorConverter()
		{
		}

		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			bool val = (bool)value;
			if (val)
			{
				return new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 0, 0));
			}
			else
			{
				return new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 255, 255));
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return false;
		}
	}
}
