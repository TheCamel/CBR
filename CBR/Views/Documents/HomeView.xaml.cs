using System.Windows.Controls;
using CBR.Core.Helpers;
using System;
using System.Windows;
using CBR.ViewModels;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Threading;

namespace CBR.Views
{
    /// <summary>
    /// Interaction logic for HomeView.xaml
    /// </summary>
    public partial class HomeView : UserControl
    {
        public HomeView()
        {
			using (new TimeLogger("HomeView.HomeView"))
			{
				InitializeComponent();
                VisualHelper.AllowFocus(this);
			}
        }

        /// <summary>
        /// process web site label click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            ProcessHelper.LaunchShellUri(e.Uri);
        }

		/// <summary>
		/// process headline click
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void RssViewer_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			Headline item = (Headline)VisualHelper.GetObjectAtPoint<ListBoxItem>((ItemsControl)sender, e.GetPosition((IInputElement)sender));

			if( item != null )
				ProcessHelper.LaunchShellUri(new Uri(item.LinkUri));
		}
    }
}
