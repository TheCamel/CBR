using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CBR.Core.Helpers;
using System.Windows.Input;
using GalaSoft.MvvmLight.CommandWpf;

namespace CBR.Components.Controls
{
	public class ProcessItem : ViewModelBaseExtended
	{
		//init data
		public bool UseTempo { get; set; }
		public DateTime StartTime { get; set; }
		public bool CanCancel { get; set; }
		public bool ShowProgress { get; set; }
		public bool ShowPercentage { get; set; }

		private string _Title;
		public string Title
		{
			get { return _Title; }
			set
			{
				if (_Title != value)
				{
					_Title = value;
					RaisePropertyChanged("Title");
				}
			}
		}

		private string _Message;
		public string Message
		{
			get { return _Message; }
			set
			{
				if (_Message != value)
				{
					_Message = value;
					RaisePropertyChanged("Message");
				}
			}
		}

		public bool WaitForCancel { get; set; }

		#region cancel command

		private ICommand cancelCommand;
		public ICommand CancelCommand
		{
			get
			{
				if (cancelCommand == null)
					cancelCommand = new RelayCommand(CancelCommandExecute, CancelCommandCanExecute);
				return cancelCommand;
			}
		}

		virtual public bool CancelCommandCanExecute()
		{
			return CanCancel;
		}

		virtual public void CancelCommandExecute()
		{
			WaitForCancel = true;
		}

		#endregion
	}
}
