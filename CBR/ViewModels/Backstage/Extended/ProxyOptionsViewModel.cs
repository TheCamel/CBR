using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CBR.Core.Helpers;
using CBR.Core.Services;

namespace CBR.ViewModels
{
	public class ProxyOptionsViewModel : ViewModelBaseExtended
	{
		#region ----------------PROPERTIES----------------

		public string Address
		{
			get
			{
				return WorkspaceService.Instance.Settings.Extended.Proxy.Address;
			}
			set
			{
				WorkspaceService.Instance.Settings.Extended.Proxy.Address = value;
			}
		}

		public string Domain
		{
			get
			{
				return WorkspaceService.Instance.Settings.Extended.Proxy.Domain;
			}
			set
			{
				WorkspaceService.Instance.Settings.Extended.Proxy.Domain = value;
			}
		}

		public string UserName
		{
			get
			{
				return WorkspaceService.Instance.Settings.Extended.Proxy.UserName;
			}
			set
			{
				WorkspaceService.Instance.Settings.Extended.Proxy.UserName = value;
			}
		}

		public string Password
		{
			get
			{
				return WorkspaceService.Instance.Settings.Extended.Proxy.Password;
			}
			set
			{
				WorkspaceService.Instance.Settings.Extended.Proxy.Password = value;
			}
		}

		public int Port
		{
			get
			{
				return WorkspaceService.Instance.Settings.Extended.Proxy.Port;
			}
			set
			{
				WorkspaceService.Instance.Settings.Extended.Proxy.Port = value;
			}
		}

		#endregion
	}
}
