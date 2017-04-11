using System.Windows;
using CBR.ViewModels;
using CBR.Core.Helpers;
using System.Windows.Controls;
using CBR.Core.Files;

namespace CBR.Views
{
    /// <summary>
    /// Interaction logic for OptionsViewExtended.xaml
    /// </summary>
    public partial class OptionsViewExtended : Window
    {
        public OptionsViewExtended()
        {
			using (new TimeLogger("OptionsViewExtended.OptionsViewExtended"))
			{
				InitializeComponent();

				this.tabItemProperties.DataContext = new DynPropertyViewModel();
				this.tabItemRegister.DataContext = new RegisterTypeViewModel();
				this.tabItemBehave.DataContext = new BehaveOptionsViewModel();
				this.tabItemProxy.DataContext = new ProxyOptionsViewModel();
                this.tabItemShare.DataContext = new ShareOptionsViewModel();
			}
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
