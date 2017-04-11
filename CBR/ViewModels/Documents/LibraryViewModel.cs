using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using CBR.Core.Helpers;
using CBR.Core.Helpers.Localization;
using CBR.Core.Models;
using CBR.Core.Services;
using GalaSoft.MvvmLight.Messaging;

namespace CBR.ViewModels
{
	public class LibraryViewModel : DocumentViewModel
	{
		#region ----------------CONSTRUCTOR----------------

		public LibraryViewModel()
		{
			this.ContentId = "LibraryViewModel";
			this.Icon = "pack://application:,,,/Resources/Images/32x32/book_type/book_type.png";

			CultureManager.Instance.UICultureChanged += new CultureEventArrived(Instance_UICultureChanged);
			DisplayName = CultureManager.Instance.GetLocalization("ByCode", "DocumentTitle.Libraries", "Libraries");

			Messenger.Default.Register<Catalog>(this, ViewModelMessages.CatalogListItemAdded, HandleCatalogAdd);
			Messenger.Default.Register<Catalog>(this, ViewModelMessages.CatalogListItemChanged, HandleCatalogChange);
			Messenger.Default.Register<Catalog>(this, ViewModelMessages.CatalogListItemRemoved, HandleCatalogRemove);

			//start task to manage ByCode translations
			Task.Factory.StartNew(() =>
			{
				try
				{
					//load catalog list here because cannot be in the app start
					CatalogService.Instance.LoadRepository();

					//on first load, ask from service
					foreach (Catalog ct in CatalogService.Instance.CatalogRepository)
						HandleCatalogChange(ct);
				}
				catch (Exception err)
				{
					LogHelper.Manage("LibraryViewModel.LibraryViewModel start loading catalogs", err);
				}
			});
		}

		/// <summary>
		/// Child classes can override this method to perform clean-up logic, such as removing event handlers.
		/// </summary>
		override public void Cleanup()
		{
			base.Cleanup();
			CultureManager.Instance.UICultureChanged -= new CultureEventArrived(Instance_UICultureChanged);

			Messenger.Default.Unregister<Catalog>(this, ViewModelMessages.CatalogListItemAdded, HandleCatalogAdd);
			Messenger.Default.Unregister<Catalog>(this, ViewModelMessages.CatalogListItemChanged, HandleCatalogChange);
			Messenger.Default.Unregister<Catalog>(this, ViewModelMessages.CatalogListItemRemoved, HandleCatalogRemove);
		}

		void Instance_UICultureChanged(object sender, CultureEventArgs e)
		{
			DisplayName = CultureManager.Instance.GetLocalization("ByCode", "DocumentTitle.Libraries", "Libraries");
		}

		#endregion

		#region ----------------PROPERTIES----------------

		new public ObservableCollection<CatalogViewModel> Data
		{
			get
			{
				if (base.Data == null)
					base.Data = new ObservableCollection<CatalogViewModel>();
				return base.Data as ObservableCollection<CatalogViewModel>;
			}
			set
			{
				if (base.Data != value)
				{
					base.Data = value;
					RaisePropertyChanged("Catalogs");
				}
			}
		}

		public ICollectionView Catalogs
		{
			get
			{
				return CollectionViewSource.GetDefaultView(Data);
			}
		}

		private string _CurrentViewMode = "LibraryImageView";
		public string CurrentViewMode
		{
			get { return _CurrentViewMode; }
			set
			{
				if (_CurrentViewMode != value)
				{
					_CurrentViewMode = value;
					RaisePropertyChanged("CurrentViewMode");
					RaisePropertyChanged("IsViewThumb");
					RaisePropertyChanged("IsViewGrid");

					RaisePropertyChanged("CurrentItemStyle");
				}
			}
		}

		public bool IsViewThumb
		{
			get { return _CurrentViewMode.Equals("LibraryImageView"); }
			set { CurrentViewMode = value == true ? "LibraryImageView" : "LibraryGridView"; }
		}
		public bool IsViewGrid
		{
			get { return _CurrentViewMode.Equals("LibraryGridView"); }
		}

		public string CurrentItemStyle
		{
			get { return IsViewGrid ? "GridViewItemStyle" : "ThumbnailViewItemStyle"; }
		}

		#endregion

		#region ----------------COMMANDS----------------

		#region forward command

		/// <summary>
		/// override the execute handler
		/// </summary>
		/// <param name="param"></param>
		/// <returns></returns>
		protected override CommandContext GetForwardCommandContext(string param)
		{
			return new CommandContext()
					{
						CommandName = param,
						CommandParameter = (this.Catalogs.CurrentItem as CatalogViewModel).Data.CatalogFilePath
					};
		}

		/// <summary>
		/// override the can execute handler
		/// </summary>
		/// <param name="param"></param>
		/// <returns></returns>
		protected override bool CanForwardCommand(string param)
		{
			if (this.Catalogs.CurrentItem != null)
				return true;
			else
				return false;
		}

		#endregion

		#endregion

		#region -----------------HANDLERS-----------------

		private void HandleCatalogAdd(Catalog o)
		{
			IEnumerable<CatalogViewModel> cvmList = Data.Where(c => c.Data.CatalogFilePath == o.CatalogFilePath);
			//exist ? update view model
			if (cvmList.Count() == 0)
				Data.Add(new CatalogViewModel(o));
		}

		private void HandleCatalogChange(Catalog o)
		{
			IEnumerable<CatalogViewModel> cvmList = Data.Where(c => c.Data.CatalogFilePath == o.CatalogFilePath);
			//exist ? then add, update is done in catalog view model
			if (cvmList.Count() == 1)
			{
				CatalogViewModel cvm = Data.Single(c => c.Data.CatalogFilePath == o.CatalogFilePath);
				if (File.Exists(o.CatalogFilePath))
					cvm.Data = o;
			}
			else
				HandleCatalogAdd(o);
		}

		private void HandleCatalogRemove(Catalog o)
		{
			IEnumerable<CatalogViewModel> cvmList = Data.Where(c => c.Data.CatalogFilePath == o.CatalogFilePath);
			//exist ? update view model
			if (cvmList.Count() == 1)
			{
				CatalogViewModel cvm = Data.Single(c => c.Data.CatalogFilePath == o.CatalogFilePath);
				Data.Remove(cvm);
			}
		}

		#endregion
	}
}
