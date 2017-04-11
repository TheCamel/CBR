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
	[ValueConversion(typeof(bool), typeof(System.Windows.WindowState))]
	public class BoolToWindowStateConverter : IValueConverter
	{
		/// <summary>
		/// Singleton access
		/// </summary>
		public static readonly BoolToWindowStateConverter Instance = new BoolToWindowStateConverter(); 

		/// <summary>
		/// Private constructor for singleton pattern
		/// </summary>
		private BoolToWindowStateConverter()
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
			bool val = false;
			bool param = true;

			if (value != null)
				val = (bool)value;
			if (parameter != null)
				param = System.Convert.ToBoolean(parameter);
			
			if (val)
			{
				// allow to test on !value, that is not possible in the binding
				if( param == true )
                    return System.Windows.WindowState.Maximized;
				else
                    return System.Windows.WindowState.Normal;
			}
			else
			{
				if (param == true)
                    return System.Windows.WindowState.Normal;
				else
                    return System.Windows.WindowState.Maximized;
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
            System.Windows.WindowState val = (System.Windows.WindowState)value;
            if (val == System.Windows.WindowState.Maximized)
			{
				return true;
			}
			else
			{
				return false;
			}
		}
	}
}
