using System.Windows;
using System.Windows.Controls;
using CBR.Core.Models;
using CBR.ViewModels;
using CBR.Core.Helpers;
using System;

namespace CBR.Views
{
	/// <summary>
	/// Interaction logic for RecentFileView.xaml
	/// </summary>
	public partial class RecentFileView : UserControl
	{
		public RecentFileView()
		{
			using (new TimeLogger("RecentFileView.RecentFileView"))
			{
				InitializeComponent();

				this.DataContext = new RecentFileViewModel();
			}
		}

		private void BookButton_Click(object sender, RoutedEventArgs e)
		{
			if (LogHelper.CanDebug())
				LogHelper.Begin("RecentFileView.BookButton_Click");
			try
			{
				MainViewModel mvm = Application.Current.MainWindow.DataContext as MainViewModel;

				RecentFileInfo rfi = ((RecentFileInfoViewModel)(sender as Button).Tag).Data;

				mvm.BackStageIsOpen = false;
				mvm.BookOpenFileCommand.Execute(System.IO.Path.Combine(rfi.FilePath, rfi.FileName));
			}
			catch (Exception err)
			{
				LogHelper.Manage("RecentFileView.BookButton_Click", err);
			}
			finally
			{
				LogHelper.End("RecentFileView.BookButton_Click");
			}  
		}

		private void CatalogButton_Click(object sender, RoutedEventArgs e)
		{
			if (LogHelper.CanDebug())
				LogHelper.Begin("RecentFileView.CatalogButton_Click");
			try
			{
				MainViewModel mvm = Application.Current.MainWindow.DataContext as MainViewModel;

				RecentFileInfo rfi = ((RecentFileInfoViewModel)(sender as Button).Tag).Data;

				mvm.BackStageIsOpen = false;
				mvm.CatalogOpenFileCommand.Execute(System.IO.Path.Combine(rfi.FilePath, rfi.FileName));
			}
			catch (Exception err)
			{
				LogHelper.Manage("RecentFileView.CatalogButton_Click", err);
			}
			finally
			{
				LogHelper.End("RecentFileView.CatalogButton_Click");
			}  
		}
	}
}
