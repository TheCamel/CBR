using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;
using System.Globalization;
using System.Windows.Controls;

namespace CBR.Components.Converters
{
    [ValueConversion(typeof(object), typeof(object))]
    public class StringToResourceConverter : IMultiValueConverter  
    {
        /// <summary>
		/// Singleton access
		/// </summary>
        public static readonly StringToResourceConverter Instance = new StringToResourceConverter(); 

		/// <summary>
		/// Private constructor for singleton pattern
		/// </summary>
        private StringToResourceConverter()
		{
		}

        /// <summary>
        /// Converts a value.
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        public object Convert(object[] value, Type targetType, object parameter, CultureInfo culture)
        {
            var view = (FrameworkElement)value[0];
			object test = view.TryFindResource(value[1].ToString());
            return view.TryFindResource(value[1].ToString());
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

    }
}
