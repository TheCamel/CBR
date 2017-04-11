using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    /// Interaction logic for LibraryView.xaml
    /// </summary>
    public partial class LibraryView : UserControl
    {
        public LibraryView()
        {
            using (new TimeLogger("LibraryView.LibraryView"))
            {
                InitializeComponent();
                VisualHelper.AllowFocus(this);
            }
        }

		#region --------------------INTERNAL--------------------

		/// <summary>
		/// handle a item double click (using Interaction.Triggers is not working because on list
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void Item_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			try
			{
				LibraryViewModel model = DataContext as LibraryViewModel;
				if (model == null)
					return;

				model.ForwardCommand.Execute("CatalogOpenFileCommand");
			}
			catch (Exception err)
			{
				LogHelper.Manage("ExplorerView:Grouping", err);
			}
		}

		#endregion
    }
}
