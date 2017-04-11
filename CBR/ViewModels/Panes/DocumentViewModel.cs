using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CBR.Core.Helpers;
using System.Windows.Input;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;

namespace CBR.ViewModels
{
	/// <summary>
	/// This ViewModelBase subclass requests to be removed 
	/// from the UI when its CloseCommand executes.
	/// </summary>
	public abstract class DocumentViewModel : PaneViewModel
	{
		#region -----------------Constructor-----------------

		protected DocumentViewModel()
		{
		}

		#endregion

		
		#region -----------------CloseCommand-----------------

		RelayCommand _closeCommand;

		/// <summary>
		/// Returns the command that, when invoked, attempts
		/// to remove this from the user interface.
		/// </summary>
		public ICommand CloseCommand
		{
			get
			{
				if (_closeCommand == null)
					_closeCommand = new RelayCommand(delegate() { this.OnRequestClose(); });

				return _closeCommand;
			}
		}

		#endregion

		#region -----------------RequestClose [event]-----------------

		/// <summary>
		/// Raised when this should be removed from the UI.
		/// </summary>
		///public event EventHandler RequestClose;

		virtual protected void OnRequestClose()
		{
            //EventHandler handler = this.RequestClose;
            //if (handler != null)
            //    handler(this, EventArgs.Empty);
            Messenger.Default.Send<DocumentViewModel>(this, ViewModelMessages.DocumentRequestClose);
		}

        #endregion
    }
}
