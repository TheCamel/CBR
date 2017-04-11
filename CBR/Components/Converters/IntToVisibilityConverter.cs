using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace CBR.Components.Converters
{
    /// <summary>
	/// Converter : Boolean to System.Windows.Visibility and not revert
	/// </summary>
    [ValueConversion(typeof(int), typeof(System.Windows.Visibility))]
    public class IntToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Singleton access
        /// </summary>
        public static readonly IntToVisibilityConverter Instance = new IntToVisibilityConverter();

        /// <summary>
        /// Private constructor for singleton pattern
        /// </summary>
        private IntToVisibilityConverter()
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
            int val = 0;
            bool param = true;

            if (value != null)
                val = (int)value;
            if (parameter != null)
                param = System.Convert.ToBoolean(parameter);

            if (val > 0)
            {
                // allow to test on !value, that is not possible in the binding
                if (param == true)
                    return System.Windows.Visibility.Hidden;
                else
                    return System.Windows.Visibility.Visible;
            }
            else
            {
                if (param == true)
                    return System.Windows.Visibility.Hidden;
                else
                    return System.Windows.Visibility.Visible;
            }
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
            return null;
        }
    }
}
