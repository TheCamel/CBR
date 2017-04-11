using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.ComponentModel;
using System.Windows;
using CBR.Core.Helpers;
using System.Windows.Input;
using System.Windows.Data;
using System.Windows.Documents;

namespace CBR.Components.Controls
{
	public class ExtendedListView : ListView
	{
		#region --------------------CONSTRUCTOR--------------------

		static ExtendedListView()
		{
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(ExtendedListView),
				new FrameworkPropertyMetadata(typeof(ExtendedListView)));
		}

		public ExtendedListView()
        {
			this.DefaultStyleKey = typeof(ExtendedListView);
        }
		#endregion

		#region --------------------DEPENDENCY PROPERTIES--------------------

		/// <summary>
		/// Drag and drop flag
		/// </summary>
		public static readonly DependencyProperty DragAndDropIDProperty =
			DependencyProperty.Register("DragAndDropID", typeof(string), typeof(ExtendedListView), null);

		/// <summary>
		/// Drag and drop flag
		/// </summary>
		public string DragAndDropID
		{
			get { return (string)GetValue(DragAndDropIDProperty); }
			set { SetValue(DragAndDropIDProperty, value); }
		}

		public static readonly DependencyProperty ColumnOrderProperty =
			DependencyProperty.Register("ColumnOrder", typeof(string), typeof(ExtendedListView), null);

		public string ColumnOrder
		{
			get { return (string)GetValue(ColumnOrderProperty); }
			set { SetValue(ColumnOrderProperty, value); }
		}

		#endregion


		#region --------------------INTERNAL--------------------

		private DragHelper _drager = null;
		private List<int> currentColumnOrder = null;

		/// <summary>
		/// Gets the parts out of the template.
		/// </summary>
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			if (!System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
			{
				if (_drager == null)
				{
					_drager = new DragHelper(this, typeof(ListViewItem));

					_drager.OnStartDrag += new StartDragEventHandler(drag_OnStartDrag);
					_drager.OnContinueDrag += new StartDragEventHandler(_drager_OnContinueDrag);

					this.AddHandler(GridViewColumnHeader.ClickEvent, new RoutedEventHandler(this.ListViewHeader_Click));
				}

				if (View is GridView && currentColumnOrder == null)
				{
					GridView grid = View as GridView;
					grid.Columns.CollectionChanged += Columns_CollectionChanged;

					currentColumnOrder = new List<int>();
					for( int i = 0; i < grid.Columns.Count; i++ )
						currentColumnOrder.Add( i );

					if (!string.IsNullOrEmpty(ColumnOrder))
						LoadColumnOrdered();
				}
			}
		}

		void Columns_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Move)
			{
				int item = currentColumnOrder[e.OldStartingIndex];
				currentColumnOrder.RemoveAt(e.OldStartingIndex);
				currentColumnOrder.Insert(e.NewStartingIndex, item);

				ColumnOrder = string.Join(",", currentColumnOrder.Select(o => o.ToString()).ToArray());
			}
		}

		private void LoadColumnOrdered()
		{
			GridView grid = View as GridView;
			var indices = ColumnOrder.Split(',');
			GridViewColumn[] tempColumns = new GridViewColumn[grid.Columns.Count];
			for ( int pos = 0; pos < grid.Columns.Count; pos++ )
			{
				tempColumns[pos] = grid.Columns[pos];
			}
			// Move columns to their proper position
			for (int i = 0; i < indices.Length - 1; i++)
			{
				int index = int.Parse(indices[i]);
				GridViewColumn col = tempColumns[index];
				// Only move column if it not in default position
				if (grid.Columns[i] != col)
				{
					grid.Columns.Remove(col);
					grid.Columns.Insert(i, col);

					currentColumnOrder.Remove(index);
					currentColumnOrder.Insert(i, index);
				}
			}

			ColumnOrder = string.Join(",", currentColumnOrder.Select(o => o.ToString()).ToArray());
		}
    
        void drag_OnStartDrag(object sender, MouseEventArgs e)
        {
            try
            {
                // Get the dragged ListViewItem
				ListViewItem item = VisualHelper.FindAnchestor<ListViewItem>((DependencyObject)e.OriginalSource);

                if (item != null)
                {
					object associatedData = this.ItemContainerGenerator.ItemFromContainer(item);

					// Find the data behind the item + Initialize the drag & drop operation
					DataObject dragData = new DataObject(DragAndDropID, associatedData);
					_drager.DoDragDrop(dragData, item);

                }
            }
            catch (Exception err)
            {
                LogHelper.Manage("ExtendedlistView:drag_OnStartDrag", err);
            }
        }

        void _drager_OnContinueDrag(object sender, MouseEventArgs e)
        {
            try
            {
                // Get the dragged ListViewItem
				ListViewItem item = VisualHelper.FindAnchestor<ListViewItem>((DependencyObject)e.OriginalSource);

                if (item != null)
                {
					object associatedData = this.ItemContainerGenerator.ItemFromContainer(item);
					_drager.DragDropContinue(associatedData != null);
                }
            }
            catch (Exception err)
            {
                LogHelper.Manage("ExtendedlistView:drag_OnStartDrag", err);
            }
        }

		/// <summary>
        /// called by the message register delegate
        /// </summary>
        public void Grouping()
        {
            try
            {
                if ((this.ItemsSource as ICollectionView).GroupDescriptions.Count > 0)
                    this.GroupStyle.Add(new GroupStyle() { ContainerStyle = (Style)FindResource("AlphaGroupContainerStyle") });
                else
                {
                    this.GroupStyle.Clear();
                }
            }
            catch (Exception err)
            {
                LogHelper.Manage("ExtendedlistView:Grouping", err);
            }
        }

		GridViewColumnHeader _lastHeaderClicked = null;
		ListSortDirection _lastDirection = ListSortDirection.Ascending;
		private SortAdorner _CurAdorner = null;
 
		private void ListViewHeader_Click(object sender, RoutedEventArgs e)
		{
			GridViewColumnHeader headerClicked = e.OriginalSource as GridViewColumnHeader;
			ListSortDirection direction;

			if (headerClicked != null && _CurAdorner != null)
			{
				AdornerLayer.GetAdornerLayer(headerClicked).Remove(_CurAdorner);
				Items.SortDescriptions.Clear();
			}
 

			if (headerClicked != null &&
				headerClicked.Role != GridViewColumnHeaderRole.Padding)
			{
				if (headerClicked != _lastHeaderClicked)
				{
					direction = ListSortDirection.Ascending;
				}
				else
				{
					if (_lastDirection == ListSortDirection.Ascending)
					{
						direction = ListSortDirection.Descending;
					}
					else
					{
						direction = ListSortDirection.Ascending;
					}
				}

				// see if we have an attached SortPropertyName value
				string sortBy = headerClicked.Tag as string;
				if (string.IsNullOrEmpty(sortBy))
				{
					// otherwise use the column header name
					sortBy = headerClicked.Column.Header as string;
				}
				Sort(sortBy, direction);

				_lastHeaderClicked = headerClicked;
				_lastDirection = direction;

				_CurAdorner = new SortAdorner(_lastHeaderClicked, direction);
				AdornerLayer.GetAdornerLayer(_lastHeaderClicked).Add(_CurAdorner);
			}
		}

		private void Sort(string sortBy, ListSortDirection direction)
		{
			ICollectionView dataView =
			  CollectionViewSource.GetDefaultView(this.ItemsSource);

			if (dataView != null)
			{
				dataView.SortDescriptions.Clear();
				SortDescription sd = new SortDescription(sortBy, direction);
				dataView.SortDescriptions.Add(sd);
				dataView.Refresh();
			}
		}

		#endregion
	}
}
