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
using CBR.Core.Models;
using CBR.ViewModels;
using CBR.Core.Helpers;
using System.ComponentModel;
using GalaSoft.MvvmLight.Messaging;

namespace CBR.Views
{
    /// <summary>
    /// Interaction logic for ExplorerView.xaml
    /// </summary>
    public partial class ExplorerView : UserControl
    {
        #region --------------------CONSTRUCTOR--------------------

        public ExplorerView()
        {
			using (new TimeLogger("ExplorerView.ExplorerView"))
			{
				InitializeComponent();
                VisualHelper.AllowFocus(this);

				if (!System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
				{
					Messenger.Default.Register<MessageBase>(this, (s) => { this.CatalogListView.Grouping(); });
				}
			}
        }
        
        #endregion

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
                ExplorerViewModel model = DataContext as ExplorerViewModel;
                if (model == null)
                    return;

                model.ForwardCommand.Execute( "BookReadCommand" );
            }
            catch (Exception err)
            {
                LogHelper.Manage("ExplorerView:Grouping", err);
            }
        }

        #endregion
    }
}
