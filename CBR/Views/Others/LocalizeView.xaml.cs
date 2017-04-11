using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using CBR.Core.Helpers.Localization;
using CBR.ViewModels.Others;

namespace CBR.Views.Others
{
    /// <summary>
    /// Interaction logic for LocalizeView.xaml
    /// </summary>
    public partial class LocalizeView : Window
    {
        #region ----------------CONSTRUCTOR----------------
        /// <summary>
        /// Constructor
        /// </summary>
        public LocalizeView()
        {
            InitializeComponent();

			this.DataContext = new LocalizeViewModel();
        }
        #endregion

		/// <summary>
		/// save all and close the dialog, no mvvm need
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void btnExit_Click(object sender, RoutedEventArgs e)
		{
			CultureManager.Instance.SaveResources();
			this.Close();
		}
    }
}
