using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CBR.Core.Helpers;
using CBR.ViewModels;
using CBR.Core.Services;

namespace CBR.Views
{
    /// <summary>
    /// Interaction logic for DriveView.xaml
    /// </summary>
    public partial class DriveView : UserControl
    {
        public DriveView()
        {
            using (new TimeLogger("DriveView.DriveView"))
            {
                InitializeComponent();
                VisualHelper.AllowFocus(this);
            }
        }

        #region --------------------LISTVIEW EVENTS--------------------

        private ListSysObjectViewModel old;

        private void listViewContent_DragEnter(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent("CBR.Book.Path"))
            {
                e.Effects = DragDropEffects.None;
                return;
            }
        }

        /// <summary>
        /// highlight drop item
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listViewContent_DragOver(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent("CBR.Book.Path"))
            {
                e.Effects = DragDropEffects.None;
                return;
            }

            ListSysObjectViewModel item = (ListSysObjectViewModel)VisualHelper.GetObjectAtPoint<ListViewItem>((ItemsControl)sender, e.GetPosition((IInputElement)sender));
            if (item != null && item is ListSysDirectoryViewModel)
            {
                if (old != null)
                    old.IsHighlighted = false;
                item.IsHighlighted = true;
                old = item;
            }
        }

        private void listViewContent_DragLeave(object sender, DragEventArgs e)
        {
            //ListViewItem tvi = sender as ListViewItem;
            //    if (tvi != null)
            //    {
            //        tvi.IsSelected = true;
            //    } 
            if (old != null)
                old.IsHighlighted = false;
        }


        /// <summary>
        /// manage drop action
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listViewContent_Drop(object sender, DragEventArgs e)
        {
            if (old != null)
                old.IsHighlighted = false;

            if (e.Data.GetDataPresent("CBR.Book.Path"))
            {
                string path = e.Data.GetData("CBR.Book.Path") as string;

                //string destFile = Path.Combine((this.FolderTree.SelectedItem as SysElementViewModel).FullPath, Path.GetFileName(path));

                //DocumentFactory.Instance.CopyToDevice(path, destFile, (this.DataContext as DriveViewModel).CurrentDriveType as DeviceInfo);

                //refresh !!
            }
        }

        /// <summary>
        /// handle a item double click (using Interaction.Triggers is not working because on list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Item_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                DriveViewModel model = DataContext as DriveViewModel;
                if (model == null)
                    return;

            }
            catch (Exception err)
            {
                LogHelper.Manage("ExplorerView:Grouping", err);
            }
        }
        #endregion
    }
}
