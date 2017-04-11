using System;
using System.Windows.Data;

namespace CBR.Components.Converters
{
	[ValueConversion(typeof(int), typeof(bool))]
	public class IntToRadioConverter : IValueConverter
	{
		/// <summary>
		/// Singleton access
		/// </summary>
        public static readonly IntToRadioConverter Instance = new IntToRadioConverter(); 

		/// <summary>
		/// Private constructor for singleton pattern
		/// </summary>
        private IntToRadioConverter()
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
            int val, param;

			val = (int)value;
            param = System.Convert.ToInt32(parameter);

            if (val == param) return true;
            else return false;
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
            if (System.Convert.ToBoolean(value) == true)
                return System.Convert.ToInt32(parameter);
            else
                return -1;
		}
	}
}
