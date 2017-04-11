using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;

namespace CBR.Components.Converters
{
    /// <summary>
    /// Convert a non localized resource in to color for the editor
    /// </summary>
    [ValueConversion(typeof(string), typeof(System.Windows.Media.SolidColorBrush))]
    class LocalizeStatusToColorConverter : IValueConverter
    {
        /// <summary>
		/// Singleton access
		/// </summary>
        public static readonly LocalizeStatusToColorConverter Instance = new LocalizeStatusToColorConverter(); 

		/// <summary>
		/// Private constructor for singleton pattern
		/// </summary>
        private LocalizeStatusToColorConverter()
		{
		}

        /// <summary>
        /// convert method
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
            string val = (string)value;
			if (val.StartsWith("#") )
			{
				return (System.Windows.Media.SolidColorBrush)Application.Current.FindResource("CbrBorderBrushSelected");
			}
			else
			{
				return (System.Windows.Media.SolidColorBrush)Application.Current.FindResource("CbrForegroundBrush");
			}
		}

        /// <summary>
        /// Not implemented
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
            throw new NotImplementedException();
		}
    }
}
