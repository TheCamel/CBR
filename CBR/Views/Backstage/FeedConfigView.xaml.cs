using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using CBR.Core.Helpers;
using CBR.Core.Models;
using CBR.Core.Services;
using CBR.ViewModels;

namespace CBR.Views
{
	/// <summary>
	/// Interaction logic for FeedConfigView.xaml
	/// </summary>
	public partial class FeedConfigView : UserControl
	{
		public FeedConfigView()
		{
			using (new TimeLogger("FeedConfigView.FeedConfigView"))
			{
				InitializeComponent();

				if (!System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
				{
					//WorkspaceService.Instance.Settings.Feed.Feeds.Clear();

					if (WorkspaceService.Instance.Settings.Feed.Feeds.Count <= 0)
					{
						WorkspaceService.Instance.Settings.Feed.Feeds = (List<FeedItemInfo>)XmlHelper.Deserialize(DirectoryHelper.Combine(CBRFolders.Dependencies, "feeds.xml"),
							WorkspaceService.Instance.Settings.Feed.Feeds.GetType());
					}
					this.DataContext = new FeedConfigViewModel(WorkspaceService.Instance.Settings.Feed);
				}
			}
		}

		private void btnSave_Click(object sender, RoutedEventArgs e)
		{
			FeedConfigViewModel dc = this.DataContext as FeedConfigViewModel;
			WorkspaceService.Instance.Settings.Feed.Feeds = dc.Feeds.ToList<FeedItemInfo>();
		}

		private void btnAdd_Click(object sender, RoutedEventArgs e)
		{
			FeedConfigViewModel dc = this.DataContext as FeedConfigViewModel;
			dc.Feeds.Add(new FeedItemInfo(this.tbName.Text, this.tbUrl.Text, this.cbLanguage.SelectedItem as string));
		}

		private void btnDelete_Click(object sender, RoutedEventArgs e)
		{
			FeedConfigViewModel dc = this.DataContext as FeedConfigViewModel;
			dc.Feeds.Remove(this.lbFeeds.SelectedItem as FeedItemInfo);
		}

		private void RadioButton_Checked(object sender, RoutedEventArgs e)
		{
			this.ctrlBrowse.Selection = string.Empty;
		}
	}
}
