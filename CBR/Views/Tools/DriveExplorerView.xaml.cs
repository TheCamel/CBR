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

namespace CBR.Views
{
    /// <summary>
    /// Interaction logic for DriveExplorerView.xaml
    /// </summary>
    public partial class DriveExplorerView : UserControl
    {
        public DriveExplorerView()
        {
            using (new TimeLogger("DriveExplorerView.DriveExplorerView"))
            {
                InitializeComponent();

                VisualHelper.AllowFocus(this);
            }
        }

        #region --------------------TREE EVENTS--------------------

        private void FolderTree_DragEnter(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent("CBR.Book.Path"))
            {
                e.Effects = DragDropEffects.None;
            }
        }

        /// <summary>
        ///  highlight drop item
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FolderTree_DragOver(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent("CBR.Book.Path"))
            {
                e.Effects = DragDropEffects.None;
            }
        }

        /// <summary>
        /// manage drop action
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FolderTree_Drop(object sender, DragEventArgs e)
        {
        }



        #endregion
    }
}
