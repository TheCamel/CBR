using System;
using System.Windows.Data;
using CBR.ViewModels;

namespace CBR.Components.Converters
{
	[ValueConversion(typeof(DocumentViewModel), typeof(DocumentViewModel))]
    public class ActiveDocumentConverter : IValueConverter
    {
		/// <summary>
		/// Singleton access
		/// </summary>
		public static readonly ActiveDocumentConverter Instance = new ActiveDocumentConverter(); 

		/// <summary>
		/// Private constructor for singleton pattern
		/// </summary>
		private ActiveDocumentConverter()
		{
		}

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
			if (value is DocumentViewModel)
                return value;

            return Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
			if (value is DocumentViewModel)
                return value;

            return Binding.DoNothing;
        }
    }
}
