using System;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using CBR.Core.Models;
using CBR.ViewModels;

namespace CBR.Components.Converters
{
    [ValueConversion(typeof(object), typeof(BitmapImage))]
	class TypeToImageConverter : IValueConverter
	{
		/// <summary>
		/// Singleton access
		/// </summary>
		public static readonly TypeToImageConverter Instance = new TypeToImageConverter(); 

		/// <summary>
		/// Private constructor for singleton pattern
		/// </summary>
        private TypeToImageConverter()
		{
		}

		/// <summary>
		/// IValueConverter.Convert DBType to Images
		/// </summary>
		/// <param name="value"></param>
		/// <param name="targetType"></param>
		/// <param name="parameter"></param>
		/// <param name="culture"></param>
		/// <returns></returns>
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
            if (value is DocumentType)
            {
                DocumentType typ = (DocumentType)value;

                if (typ == DocumentType.RARBased)
					return new BitmapImage(new Uri("pack://application:,,,/Resources/Images/32x32/book_type/book_type_rar.png"));
                if (typ == DocumentType.ZIPBased)
					return new BitmapImage(new Uri("pack://application:,,,/Resources/Images/32x32/book_type/book_type_zip.png"));
                if (typ == DocumentType.XPS)
					return new BitmapImage(new Uri("pack://application:,,,/Resources/Images/32x32/book_type/book_type_xps.png"));
                if (typ == DocumentType.ePUB)
					return new BitmapImage(new Uri("pack://application:,,,/Resources/Images/32x32/book_type/book_type_epub.png"));
				if (typ == DocumentType.PDF)
					return new BitmapImage(new Uri("pack://application:,,,/Resources/Images/32x32/book_type/book_type_pdf.png"));
				if (typ == DocumentType.ImageFile)
					return new BitmapImage(new Uri("pack://application:,,,/Resources/Images/32x32/book_type/book_type_img.png"));

				return new BitmapImage(new Uri("/Resources/Images/32x32/book_type/book_type_none.png", UriKind.Relative));
            }
            if (value is SysElementType)
            {
                SysElementType typ = (SysElementType)value;

                if (typ == SysElementType.Drive)
                    return new BitmapImage(new Uri("pack://application:,,,/Resources/Images/16x16/disk.png"));
                if (typ == SysElementType.Folder)
                    return new BitmapImage(new Uri("pack://application:,,,/Resources/Images/16x16/folder.png"));

                return new BitmapImage(new Uri("pack://application:,,,/Resources/Images/16x16/file.png"));
            }

            return null;
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
