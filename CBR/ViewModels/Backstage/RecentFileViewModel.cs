using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using CBR.Core.Helpers;
using CBR.Core.Models;
using CBR.Core.Services;
using System.IO;
using GalaSoft.MvvmLight.Messaging;

namespace CBR.ViewModels
{
	class RecentFileViewModel : ViewModelBaseExtended
    {
        #region ----------------CONSTRUCTOR----------------

        public RecentFileViewModel()
		{
			//register to the mediator for messages
			Messenger.Default.Register<RecentFileInfo>(this, ViewModelMessages.RecentFileChanged,
				(RecentFileInfo o) =>
				{
                    foreach (RecentFileInfoViewModel cat in Catalogs)
                        if (cat.Data == o ) Catalogs = null;

                    foreach( RecentFileInfoViewModel bk in Books )
                        if (bk.Data == o) Books = null;

				} );

			Messenger.Default.Register<List<RecentFileInfo>>(this, ViewModelMessages.RecentListChanged,
				(List<RecentFileInfo> o) =>
                {
                    if (WorkspaceService.Instance.Settings.RecentCatalogList == o)
                        Catalogs = null;

                    if (WorkspaceService.Instance.Settings.RecentFileList == o)
                        Books = null;

                } );
		}

		override public void Cleanup()
		{
			base.Cleanup();

			Messenger.Default.Unregister(this);
		}

        #endregion

        #region ----------------PROPERTIES----------------

        private ObservableCollection<RecentFileInfoViewModel> _Catalogs = null;
        /// <summary>
        /// Book object list in the catalog
        /// </summary>
        public ObservableCollection<RecentFileInfoViewModel> Catalogs
        {
            get
            {
                if (_Catalogs == null)
                {
                    _Catalogs = new ObservableCollection<RecentFileInfoViewModel>();
                    List<RecentFileInfo> temp = new List<RecentFileInfo>();

                    foreach (RecentFileInfo rfi in WorkspaceService.Instance.Settings.RecentCatalogList)
                    {
                        if (File.Exists(Path.Combine(rfi.FilePath, rfi.FileName)))
                            _Catalogs.Add(new RecentFileInfoViewModel(rfi));
                        else
                            temp.Add(rfi);
                    }

                    foreach (RecentFileInfo rfi in temp)
                        WorkspaceService.Instance.Settings.RecentCatalogList.Remove(rfi);
                }
                return _Catalogs;
            }
            set
            {
                _Catalogs = value;
                RaisePropertyChanged("RecentCatalogs");
            }
        }

        public ICollectionView RecentCatalogs
        {
            get
            {
                if (Catalogs != null)
                {
                    ICollectionView view = CollectionViewSource.GetDefaultView(Catalogs);
                    if (view.SortDescriptions.Count == 0)
                    {
                        view.GroupDescriptions.Add(new PropertyGroupDescription("IsPined"));
                        view.SortDescriptions.Add(new SortDescription("IsPined", ListSortDirection.Descending));
                        view.SortDescriptions.Add(new SortDescription("FileName", ListSortDirection.Ascending));
                    }
                    return view;
                }
                else
                    return null;
            }
        }

        private ObservableCollection<RecentFileInfoViewModel> _Books = null;
        /// <summary>
        /// Book object list in the catalog
        /// </summary>
        public ObservableCollection<RecentFileInfoViewModel> Books
        {
            get
            {
                if (_Books == null)
                {
                    _Books = new ObservableCollection<RecentFileInfoViewModel>();
                    List<RecentFileInfo> temp = new List<RecentFileInfo>();

                    foreach (RecentFileInfo rfi in WorkspaceService.Instance.Settings.RecentFileList)
                    {
                        if (File.Exists(Path.Combine(rfi.FilePath, rfi.FileName)))
                            _Books.Add(new RecentFileInfoViewModel(rfi));
                        else
                            temp.Add(rfi);
                    }

                    foreach (RecentFileInfo rfi in temp)
                        WorkspaceService.Instance.Settings.RecentFileList.Remove(rfi);

                }
                return _Books;
            }
            set
            {
                _Books = value;
                RaisePropertyChanged("RecentBooks");
            }
        }

        public ICollectionView RecentBooks
        {
            get
            {
                if (Books != null)
                {
                    ICollectionView view = CollectionViewSource.GetDefaultView(Books);
                    if (view.SortDescriptions.Count == 0)
                    {
                        view.GroupDescriptions.Add(new PropertyGroupDescription("IsPined"));
                        view.SortDescriptions.Add(new SortDescription("IsPined", ListSortDirection.Descending));
                        view.SortDescriptions.Add(new SortDescription("FileName", ListSortDirection.Ascending));
                    }
                    return view;
                }
                else
                    return null;
            }
        }

        #endregion
    }
}
