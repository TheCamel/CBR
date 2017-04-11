using System.Globalization;
using System.IO;
using CBR.Core.Helpers;
using CBR.Core.Helpers.Localization;

namespace CBR.ViewModels
{
    public class LanguageMenuItemViewModel : MenuItemViewModel
    {
        #region ----------------CONSTRUCTOR----------------

        public LanguageMenuItemViewModel()
        {
        }

        public LanguageMenuItemViewModel( CultureInfo info ) : base( info )
		{
			IsChecked = info.IetfLanguageTag == CultureManager.Instance.UICulture.IetfLanguageTag;
		}

		#endregion

        #region ----------------PROPERTIES----------------

        new public CultureInfo Data
        {
			get { return base.Data as CultureInfo; }
            set { base.Data = value; }
        }

        new public string ToDisplay
        {
            get
			{
				return string.Format( "{0} - {1}", Data.DisplayName, Data.ThreeLetterWindowsLanguageName);
			}
		}


        public string Icon
		{
			get
			{
				return DirectoryHelper.Combine(CBRFolders.Language, Data.IetfLanguageTag + ".png");
			}
		}

        #endregion

    }
}
