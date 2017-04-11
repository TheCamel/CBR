using CBR.Core.Helpers;
using CBR.Core.Models;

namespace CBR.ViewModels
{
    public class RegFileTypeViewModel : MenuItemViewModel
    {
        #region ----------------CONSTRUCTOR----------------

        public RegFileTypeViewModel()
        {
        }

        public RegFileTypeViewModel(DocumentInfo info)
            : base(info)
		{
            IsChecked = RegisterFiletype.IsRegisteredToMe(Extension);
		}

		#endregion

        #region ----------------PROPERTIES----------------

        public string Extension
        {
            get { return Data.Extension.Replace(".", ""); }
        }
        
        new public DocumentInfo Data
        {
            get { return base.Data as DocumentInfo; }
            set { base.Data = value; }
        }

        new public string ToDisplay
        {
            get { return Data.DialogDescription; }
        }

        public string Icon { get { return Data.IconFile; } }

        #endregion

        #region ----------------COMMANDS----------------

        override protected void ExecCheckCommand(string param)
        {
            if (RegisterFiletype.IsRegisteredToMe(Extension))
                RegisterFiletype.UnRegister(Extension);
            else
                RegisterFiletype.RegisterToMe(Extension, ToDisplay, Icon); 
        }
        #endregion
    }
}
