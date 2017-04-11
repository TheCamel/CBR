using System.Windows.Controls;
using System.Windows.Input;
using CBR.ViewModels;
using CBR.Core.Formats.OPDS;
using CBR.Core.Helpers;

namespace CBR.Views
{
	/// <summary>
	/// Interaction logic for FeedView.xaml
	/// </summary>
    public partial class FeedView : UserControl
	{
		public FeedView()
		{
			using (new TimeLogger("FeedView.FeedView"))
			{
				InitializeComponent();
                VisualHelper.AllowFocus(this);
			}
		}

		private void ListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			if (this.feedItemList.SelectedItem is OpdsCategory)
			{
				FeedViewModel vm = this.DataContext as FeedViewModel;
				vm.ResetHistory();
				vm.NavigateCommand.Execute((this.feedItemList.SelectedItem as OpdsCategory).Link);
			}
		}
	}
}
