using System;
using System.Windows.Input;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;

namespace CBR.Core.Helpers
{
	public class MenuItemViewModel : ViewModelBaseExtended
    {
        #region ----------------CONSTRUCTOR----------------

        public MenuItemViewModel()
        {
        }

        public MenuItemViewModel( object data )
		{
            Data = data;
		}

		#endregion

        #region ----------------PROPERTIES----------------

        public string ToDisplay { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsChecked { get; set; }
        
        #endregion

        #region ----------------COMMANDS----------------

        #region check command
        private ICommand checkCommand;
        public ICommand CheckCommand
        {
            get
            {
                if (checkCommand == null)
                    checkCommand = new RelayCommand<string>(ExecCheckCommand,
                        delegate(string param)
                        {
                            return Data != null;
                        });
                return checkCommand;
            }
        }

        virtual protected void ExecCheckCommand(string param)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region generic command
        private ICommand genericCommand;
        public ICommand GenericCommand
        {
            get
            {
                if (genericCommand == null)
                    genericCommand = new RelayCommand<string>(ExecCommand);
                return genericCommand;
            }
        }

        void ExecCommand(string param)
        {
            Messenger.Default.Send(ViewModelBaseMessages.MenuItemCommand, this.Data);
        }
        #endregion

        #endregion
    }
}
