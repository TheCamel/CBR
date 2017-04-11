using System.Collections.Generic;
using System.Linq;
using CBR.Core.Helpers;
using CBR.Core.Services;

namespace CBR.ViewModels
{
	public class RegisterTypeViewModel : ViewModelBaseExtended
    {
        public List<RegFileTypeViewModel> FileTypes
        {
            get
            {
                return DocumentFactory.Instance.BookExtensionRegister.Select( p => new RegFileTypeViewModel(p)).ToList();
            }
        }
    }
}
