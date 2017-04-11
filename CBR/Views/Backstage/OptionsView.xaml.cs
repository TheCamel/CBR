using System.Windows;
using System.Windows.Controls;
using CBR.ViewModels;
using CBR.Core.Helpers;

namespace CBR.Views
{
	/// <summary>
	/// Interaction logic for OptionsView.xaml
	/// </summary>
	public partial class OptionsView : UserControl
	{
		public OptionsView()
		{
			using (new TimeLogger("OptionsView.OptionsView"))
			{
				InitializeComponent();

				this.DataContext = new OptionsViewModel();
			}
		}

        private void btnExtendedOptions_Click(object sender, RoutedEventArgs e)
        {
            OptionsViewExtended view = new OptionsViewExtended();
            view.ShowDialog();
        }
	}
}
