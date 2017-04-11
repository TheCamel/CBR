using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace CBR.Components.Converters
{
    [ValueConversion(typeof(bool), typeof(bool))]
    public class InvertBooleanConverter : IValueConverter
    {
        /// <summary>
		/// Singleton access
		/// </summary>
        public static readonly InvertBooleanConverter Instance = new InvertBooleanConverter(); 

		/// <summary>
		/// Private constructor for singleton pattern
		/// </summary>
        private InvertBooleanConverter()
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
			if (value != null)
				return !(bool)value;
			else
				return false;
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
            if (value != null)
                return !(bool)value;
            else
                return false;
        }
    }
}
