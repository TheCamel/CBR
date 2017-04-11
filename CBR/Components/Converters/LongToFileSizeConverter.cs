using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using CBR.Core.Helpers.Localization;

namespace CBR.Components.Converters
{
	/// <summary>
	/// Converter : long to string (no revert) with unit in
	/// </summary>
	[ValueConversion(typeof(long), typeof(string))]
	public class LongToFileSizeConverter : IValueConverter
	{
		/// <summary>
		/// Singleton access
		/// </summary>
		public static readonly LongToFileSizeConverter Instance = new LongToFileSizeConverter(); 

		/// <summary>
		/// Private constructor for singleton pattern
		/// </summary>
		private LongToFileSizeConverter()
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
			if (value is long && targetType == typeof(string))
			{
				long val = (long)value;
				if (val > 1048576) // if more than mega
					return val / 1048576 + CultureManager.Instance.GetLocalization("ByCode", "MEGA", "(Mb)");
				else
					return val / 1024 + CultureManager.Instance.GetLocalization("ByCode", "KILO", "(Kb)");
			}
			return string.Empty;
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
			throw new NotImplementedException();
		}
	}
}
