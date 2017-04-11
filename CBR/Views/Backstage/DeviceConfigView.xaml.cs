using System.Linq;
using System.Windows;
using System.Windows.Controls;
using CBR.Core.Helpers;
using CBR.Core.Services;
using CBR.ViewModels;
using System.IO;
using System.Collections.Generic;
using CBR.Core.Models;

namespace CBR.Views
{
	/// <summary>
	/// Interaction logic for DeviceConfigView.xaml
	/// </summary>
	public partial class DeviceConfigView : UserControl
	{
		public DeviceConfigView()
		{
			using (new TimeLogger("DeviceConfigView.DeviceConfigView"))
			{
				InitializeComponent();

				if (!System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
				{
					//WorkspaceService.Instance.Settings.DeviceInfoList.Clear();

					if (WorkspaceService.Instance.Settings.DeviceInfoList.Count <= 0)
					{
						WorkspaceService.Instance.Settings.DeviceInfoList = (List<DeviceInfo>)XmlHelper.Deserialize(DirectoryHelper.Combine(CBRFolders.Dependencies, "devices.xml"),
							WorkspaceService.Instance.Settings.DeviceInfoList.GetType());
					}
					this.DataContext = new DeviceConfigViewModel(WorkspaceService.Instance.Settings.DeviceInfoList);
				}
			}
		}

		private void btnSave_Click(object sender, RoutedEventArgs e)
		{
			DeviceConfigViewModel dc = this.DataContext as DeviceConfigViewModel;
			WorkspaceService.Instance.Settings.DeviceInfoList = dc.Data.ToList<Core.Models.DeviceInfo>();
		}

		private void btnAddDevice_Click(object sender, RoutedEventArgs e)
		{
			DeviceConfigViewModel dc = this.DataContext as DeviceConfigViewModel;
			dc.Data.Add(new Core.Models.DeviceInfo(this.tbModel.Text, this.tbManufacturer.Text));
		}

		private void btnDeleteDevice_Click(object sender, RoutedEventArgs e)
		{
			DeviceConfigViewModel dc = this.DataContext as DeviceConfigViewModel;
			dc.Data.Remove(this.lbDevices.SelectedItem as DeviceInfo);
		}
	}
}
