using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using CBR.Core.Helpers;
using CBR.Core.Models;
using CBR.Core.Services;
using System.Windows;
using CBR.Views;
using CBR.Core.Helpers.NET.Properties;
using CBR.Core.Helpers.Localization;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;

namespace CBR.ViewModels
{
	public class ExplorerViewModel : ToolViewModel
    {
        #region ----------------CONSTRUCTOR----------------

		/// <summary>
		/// Constructor
		/// </summary>
        public ExplorerViewModel()
			: base( CultureManager.Instance.GetLocalization( "ByCode", "ExplorerView.Title", "Library Explorer") )
		{
            this.ContentId = "ExplorerViewModel";
			this.Icon = "pack://application:,,,/Resources/Images/32x32/book_type/book_type.png";

			CultureManager.Instance.UICultureChanged += new CultureEventArrived(Instance_UICultureChanged);

			//register to the mediator for messages
			Messenger.Default.Register<Catalog>(this, ViewModelMessages.CatalogChanged, HandleCatalogChange);
			Messenger.Default.Register<PropertyModel>(this, ViewModelMessages.ExplorerSortChanged, HandleExplorerSortChange);
			Messenger.Default.Register<PropertyModel>(this, ViewModelMessages.ExplorerGroupChanged, HandleExplorerGroupChange);
			Messenger.Default.Register<string>(this, ViewModelMessages.ExplorerViewModeChanged, HandleExplorerViewModeChange);
			Messenger.Default.Register<WorkspaceInfo>(this, ViewModelMessages.ExtendedSettingsChanged, HandleExtendedSettingsChange);
		}

		/// <summary>
		/// Child classes can override this method to perform clean-up logic, such as removing event handlers.
		/// </summary>
		override public void Cleanup()
		{
			base.Cleanup();
			CultureManager.Instance.UICultureChanged -= new CultureEventArrived(Instance_UICultureChanged);

			Messenger.Default.Unregister<Catalog>(this, ViewModelMessages.CatalogChanged, HandleCatalogChange);

			Messenger.Default.Unregister<PropertyModel>(this, ViewModelMessages.ExplorerSortChanged, HandleExplorerSortChange);
			Messenger.Default.Unregister<PropertyModel>(this, ViewModelMessages.ExplorerGroupChanged, HandleExplorerGroupChange);

			Messenger.Default.Unregister<string>(this, ViewModelMessages.ExplorerViewModeChanged, HandleExplorerViewModeChange);
			Messenger.Default.Unregister<WorkspaceInfo>(this, ViewModelMessages.ExtendedSettingsChanged, HandleExtendedSettingsChange);
		}

		#endregion

        #region ----------------PROPERTIES----------------

        /// <summary>
        /// owns a copy from MainViewModel
        /// </summary>
        new public Catalog Data
        {
            get { return base.Data as Catalog; }
            set
            {
                if (base.Data != value)
                {
                    base.Data = value;
                    RaisePropertyChanged("Books");
                    RaisePropertyChanged("IsHeaderEnabled");
					_viewModeCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsHeaderEnabled
        {
            get { return Data != null; }
        }

        /// <summary>
        /// the collection for list binding
        /// </summary>
        public ICollectionView Books
        {
            get
            {
                if (Data != null)
                {
                    return CollectionViewSource.GetDefaultView(Data.Books);
                }
                else
                    return null;
            }
        }

        private string _searchedText = string.Empty;
        /// <summary>
        /// filter string from ribbon
        /// </summary>
        public string SearchedText
        {
            get { return _searchedText; }
            set
            {
                _searchedText = value;

                Books.Filter = delegate(object obj)
                {
                    if (String.IsNullOrEmpty(_searchedText))
                        return true;

                    Book bk = obj as Book;
                    if (bk == null)
                        return false;

                    return (bk.FileName.IndexOf(_searchedText, 0, StringComparison.InvariantCultureIgnoreCase) > -1);
                };
            }
        }

		private string _CurrentViewMode = WorkspaceService.Instance.Settings.Extended.DefaultExplorerView;
        public string CurrentViewMode
        {
            get { return _CurrentViewMode; }
            set
            {
                if (_CurrentViewMode != value)
                {
                    _CurrentViewMode = value;
                    RaisePropertyChanged("CurrentViewMode");
                    RaisePropertyChanged("IsViewThumbSimple");
                    RaisePropertyChanged("IsViewThumbDetails");
                    RaisePropertyChanged("IsViewGrid");

                    RaisePropertyChanged("CurrentItemStyle");
                }
            }
        }

        public bool IsViewThumbSimple
        {
            get { return _CurrentViewMode.Equals("ExplorerImageView"); }
        }
        public bool IsViewThumbDetails
        {
            get { return _CurrentViewMode.Equals("ExplorerImageDetailView"); }
        }
        public bool IsViewGrid
        {
            get { return _CurrentViewMode.Equals("ExplorerGridView"); }
        }

        public string CurrentItemStyle
        {
			get { return IsViewGrid ? "GridViewItemStyle" : "ThumbnailViewItemStyle"; }
        }

		private bool _ShowTooltipExplorer = WorkspaceService.Instance.Settings.Extended.ShowTooltipExplorer;
		public bool ShowTooltipExplorer
		{
			get { return _ShowTooltipExplorer; }
			set
			{
				if (_ShowTooltipExplorer != value)
				{
					_ShowTooltipExplorer = value;
					RaisePropertyChanged("ShowTooltipExplorer");
				}
			}
		}

		private bool _ShowGridCover = WorkspaceService.Instance.Settings.Extended.ShowGridCover;
		public bool ShowGridCover
		{
			get { return _ShowGridCover; }
            set
            {
                if (_ShowGridCover != value)
                {
                    _ShowGridCover = value;
                    RaisePropertyChanged("ShowGridCover");
                }
            }
		}

		private List<PropertyViewModel> _sortProperties = null;
        /// <summary>
        /// sort dropdown menu items
        /// </summary>
		public List<PropertyViewModel> SortProperties
        {
			get
			{
				if (_sortProperties == null)
				{
					_sortProperties = new PropertyHelper().GetSortViewModelsWithDyn(typeof(Book));
				}
				return _sortProperties;
			}
        }

		private List<PropertyViewModel> _groupProperties = null;
        /// <summary>
        /// group dropdown menu items
        /// </summary>
        public List<PropertyViewModel> GroupProperties
        {
            get
			{
				if (_groupProperties == null)
				{
					_groupProperties = new PropertyHelper().GetGroupViewModelsWithDyn(typeof(Book));
				}
				return _groupProperties;
			}
        }

        #endregion

        #region ---------------- COMMANDS ----------------

        #region forward command

		protected override CommandContext GetForwardCommandContext(string param)
		{
			return new CommandContext()
			{
				CommandName = param,
				CommandParameter = this.Books.CurrentItem
			};
		}

		protected override bool CanForwardCommand(string param)
		{
			if (Data != null && Books.CurrentItem != null)
			{
				return true;
			}
			return false;
		}

		#endregion

		#region view mode command
		private RelayCommand<string> _viewModeCommand;
        public ICommand ViewModeCommand
        {
            get
            {
                if (_viewModeCommand == null)
                    _viewModeCommand = new RelayCommand<string>(
                        delegate(string param) { CurrentViewMode =(string)param; },
                        delegate(string param)
                        {
                            return Data != null;
                        });
                return _viewModeCommand;
            }
        }
        #endregion

		#region selection changed command
		private ICommand _selectionChangedCommand;
		public ICommand SelectionChangedCommand
		{
			get
			{
				if (_selectionChangedCommand == null)
					_selectionChangedCommand = new RelayCommand(
						delegate()
						{
							if (Books != null)
								Messenger.Default.Send<Book>(Books.CurrentItem as Book, ViewModelMessages.BookSelected);
						});
				return _selectionChangedCommand;
			}
		}
		#endregion
       
        #endregion

		#region ----------------HANDLERS----------------
		
		private void Instance_UICultureChanged(object sender, CultureEventArgs e)
		{
			DisplayName = CultureManager.Instance.GetLocalization("ByCode", "ExplorerView.Title", "Library Explorer");
		}

		private void HandleExtendedSettingsChange(WorkspaceInfo o)
		{
			ShowTooltipExplorer = WorkspaceService.Instance.Settings.Extended.ShowTooltipExplorer;
			ShowGridCover = WorkspaceService.Instance.Settings.Extended.ShowGridCover;
		}

		private void HandleExplorerViewModeChange(Object o)
		{
			CurrentViewMode = o as string;
		}

		private void HandleExplorerGroupChange(PropertyModel o)
		{
			Group(o);
		}

		private void HandleExplorerSortChange(PropertyModel o)
		{
			Sort( o );
		}

		private void HandleCatalogChange(Catalog o)
		{
			Data = o;
		}

		#endregion

		#region ----------------INTERNALS----------------

		internal void Sort(PropertyModel model)
        {
            PropertyViewModel prop = SortProperties.Find(p => p.Data == model);

            IEnumerable<SortDescription> result =
                Books.SortDescriptions.Cast<SortDescription>().Where(p => p.PropertyName == prop.Data.FullName);

            if (result != null && result.Count() == 1)
            {
                Books.SortDescriptions.Remove(result.First());
            }
            else
            {
				Books.SortDescriptions.Add(new SortDescription(prop.Data.FullName, ListSortDirection.Ascending));
            }

            RaisePropertyChanged("Books");
        }

        internal void Group(PropertyModel model)
        {
            PropertyViewModel prop = GroupProperties.Find(p => p.Data == model);

            IEnumerable<PropertyGroupDescription> result =
				Books.GroupDescriptions.Cast<PropertyGroupDescription>().Where(p => p.PropertyName == prop.Data.FullName);

            if (result != null && result.Count() == 1)
            {
                Books.GroupDescriptions.Remove(result.First());
            }
            else
            {
				Books.GroupDescriptions.Add(new PropertyGroupDescription(prop.Data.FullName));
            }

            Messenger.Default.Send<MessageBase>( new MessageBase(this) );

            RaisePropertyChanged("Books");
        }

        #endregion
    }
}
