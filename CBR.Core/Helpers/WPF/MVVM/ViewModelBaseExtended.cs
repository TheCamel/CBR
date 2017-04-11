using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;

namespace CBR.Core.Helpers
{
	public class ViewModelBaseExtended : ViewModelBase
	{
		#region DisplayName

		private string _DisplayName;
		/// <summary>
		/// Returns the user-friendly name of this object.
		/// Child classes can set this property to a new value,
		/// or override it to determine the value on-demand.
		/// </summary>
		public virtual string DisplayName
		{
			get { return _DisplayName; }
			protected set
			{
				if (_DisplayName != value)
				{
					_DisplayName = value;
					this.RaisePropertyChanged("DisplayName");
				}
			}
		}
		#endregion

		#region Data

		/// <summary>
		/// model data associated to a viewmodel
		/// </summary>
		public object Data { get; set; }

		#endregion

		#region ----------------COMMANDS----------------

		private ICommand forwardCommand;
		public ICommand ForwardCommand
		{
			get
			{
				if (forwardCommand == null)
					forwardCommand = new RelayCommand<string>(
						delegate(string param)
						{
							Messenger.Default.Send<CommandContext>(GetForwardCommandContext(param),
								ViewModelBaseMessages.ContextCommand );
						},
						delegate(string param)
						{
							return CanForwardCommand(param);
						});
				return forwardCommand;
			}
		}

		protected virtual CommandContext GetForwardCommandContext(string param)
		{
			return new CommandContext();
		}

		protected virtual bool CanForwardCommand(string param)
		{
			return false;
		}

		#endregion
	}

	public class CommandContext
	{
		public string CommandName { get; set; }
		public object CommandParameter { get; set; }
	}
}
