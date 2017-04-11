using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows.Input;

namespace CBR.Core.Helpers
{
	public abstract class ViewModelBase : BindableObject, IDisposable
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
					RaisePropertyChanged("DisplayName");
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

		#region IDisposable Members

		/// <summary>
		/// Invoked when this object is being removed from the application
		/// and will be subject to garbage collection.
		/// </summary>
		public void Dispose()
		{
			this.OnDispose();
		}

		/// <summary>
		/// Child classes can override this method to perform 
		/// clean-up logic, such as removing event handlers.
		/// </summary>
		protected virtual void OnDispose()
		{
		}

#if DEBUG
		/// <summary>
		/// Useful for ensuring that ViewModel objects are properly garbage collected.
		/// </summary>
		~ViewModelBase()
		{
			string msg = string.Format("{0} ({1}) ({2}) Finalized", this.GetType().Name, this.DisplayName, this.GetHashCode());
			System.Diagnostics.Debug.WriteLine(msg);
		}
#endif

		#endregion // IDisposable Members

		#region ----------------COMMANDS----------------

		private ICommand forwardCommand;
		public ICommand ForwardCommand
		{
			get
			{
				if (forwardCommand == null)
					forwardCommand = new DelegateCommand<string>(
						delegate(string param)
						{
							Mediator.Instance.NotifyColleagues<CommandContext>(ViewModelBaseMessages.ContextCommand,
								GetForwardCommandContext(param) );
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

    public class ViewModelBaseMessages
    {
		/// <summary>
		/// from explorer and home to main
		/// </summary>
		public const string ContextCommand = "ContextCommand";
        /// <summary>
        /// Active document changed
        /// </summary>
        public const string DocumentRequestClose = "DocumentRequestClose";
        /// <summary>
        /// Active document changed
        /// </summary>
        public const string DocumentActivChanged = "DocumentActivChanged";
        /// <summary>
        /// 
        /// </summary>
        public const string MenuItemCommand = "MenuItemCommand";

        /// <summary>
        /// when a book or catalog file is added/removed, notify the recent file backstage panel
        /// </summary>
        public const string RecentListChanged = "RecentListChanged";

        /// <summary>
        /// when a book or catalog file is changed, notify the recent file backstage panel
        /// </summary>
        public const string RecentFileChanged = "RecentFileChanged";

        /// <summary>
        /// when catalog list is changed
        /// </summary>
        public const string CatalogListItemChanged = "CatalogListItemChanged";

        /// <summary>
        /// when catalog list is changed
        /// </summary>
        public const string CatalogListItemAdded = "CatalogListItemAdded";

        /// <summary>
        /// when catalog list is changed
        /// </summary>
        public const string CatalogListItemRemoved = "CatalogListItemRemoved";


		/// <summary>
		/// when catalog list is changed
		/// </summary>
		public const string CatalogRefreshCover = "CatalogRefreshCover";
    }

    public class CommandContext
    {
        public string CommandName { get; set; }
        public object CommandParameter { get; set; }
    }
}
