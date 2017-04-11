using System;
using System.Globalization;
using System.Windows.Data;

namespace CBR.Components.Converters
{
	/// <summary>
	/// Used in the status bar to convert the holded scale value to percent and backwards
	/// </summary>
	[ValueConversion(typeof(double), typeof(double))]
    public class ScaleToPercentConverter : IValueConverter
    {
		/// <summary>
		/// Singleton access
		/// </summary>
		public static readonly ScaleToPercentConverter Instance = new ScaleToPercentConverter(); 

		/// <summary>
		/// Private constructor for singleton pattern
		/// </summary>
		private ScaleToPercentConverter()
		{
		}

       /// <summary>
		///  Convert a fraction to a percentage.
       /// </summary>
       /// <param name="value"></param>
       /// <param name="targetType"></param>
       /// <param name="parameter"></param>
       /// <param name="culture"></param>
       /// <returns></returns>
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Round to an integer value whilst converting.
            return (double)(int)((double)value * 100.0);
        }

        /// <summary>
		///  Convert a percentage back to a fraction.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (double)value / 100.0;
        }
    }
}
