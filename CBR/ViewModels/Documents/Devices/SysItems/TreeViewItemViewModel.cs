using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CBR.Core.Helpers;
using System.Collections.ObjectModel;
using GalaSoft.MvvmLight.Messaging;

namespace CBR.ViewModels
{
	public class TreeViewItemViewModel : ViewModelBaseExtended
    {
        static private TreeViewItemViewModel _dummyChild = new TreeViewItemViewModel();
        private TreeViewItemViewModel _parentItem;
        
        #region ----------------CONSTRUCTOR----------------

        protected TreeViewItemViewModel(TreeViewItemViewModel parent, bool lazyLoadChildren)
        {
            _parentItem = parent;

            Children = new ObservableCollection<TreeViewItemViewModel>();

            if (lazyLoadChildren)
                Children.Add(_dummyChild);
        }

        // This is used to create the DummyChild instance.
        private TreeViewItemViewModel()
        {
        }

        #endregion

        #region ----------------PROPERTIES----------------

        public ObservableCollection<TreeViewItemViewModel> Children
        {
            get;
            set;
        }

        /// <summary>
        /// Returns true if this object's Children have not yet been populated.
        /// </summary>
        public bool HasDummyChild
        {
            get { return this.Children.Count == 1 && this.Children[0] == _dummyChild; }
        }

        #region ----------------IsExpanded----------------

        private bool _isExpanded;

        /// <summary>
        /// Gets/sets whether the TreeViewItem 
        /// associated with this object is expanded.
        /// </summary>
        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                if (value != _isExpanded)
                {
                    _isExpanded = value;
                    this.RaisePropertyChanged("IsExpanded");
                }

                // Expand all the way up to the root.
                if (_isExpanded && _parentItem != null)
                    _parentItem.IsExpanded = true;

                // Lazy load the child items, if necessary.
                if (this.HasDummyChild)
                {
                    this.Children.Remove(_dummyChild);
                    this.LoadChildren();
                }
            }
        }

        #endregion

        #region ----------------IsSelected----------------

        private bool _isSelected;

        /// <summary>
        /// Gets/sets whether the TreeViewItem 
        /// associated with this object is selected.
        /// </summary>
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (value != _isSelected)
                {
                    _isSelected = value;
                    this.RaisePropertyChanged("IsSelected");

                    // Lazy load the child items, if necessary.
                    if (this.HasDummyChild)
                    {
                        this.Children.Remove(_dummyChild);
                        this.LoadChildren();
                    }

                    Messenger.Default.Send<string>(ViewModelMessages.DeviceContentChanged, this.Data as string);
                }
            }
        }

        #endregion

        #endregion

        #region ----------------CHILD LOADING----------------

        /// <summary>
        /// Invoked when the child items need to be loaded on demand.
        /// Subclasses can override this to populate the Children collection.
        /// </summary>
        protected virtual void LoadChildren()
        {
        }

        #endregion
    }
}
