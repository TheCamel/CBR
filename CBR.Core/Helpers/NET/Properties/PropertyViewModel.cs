using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using CBR.Core.Helpers.Localization;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;

namespace CBR.Core.Helpers.NET.Properties
{
	public class PropertyViewModel : ViewModelBaseExtended
	{
		#region ----------------CONSTRUCTOR----------------

		public PropertyViewModel(PropertyModel model)
		{
			Data = model;

			CultureManager.Instance.UICultureChanged += new CultureEventArrived(Instance_UICultureChanged);
			DisplayName = CultureManager.Instance.GetLocalization("ByCode", Data.LabelKey, Data.LabelKey);
		}

		void Instance_UICultureChanged(object sender, CultureEventArgs e)
		{
			DisplayName = CultureManager.Instance.GetLocalization("ByCode", Data.LabelKey, Data.LabelKey);
		}

		#endregion

		#region ----------------PROPERTIES----------------

		new public PropertyModel Data
		{
			get { return base.Data as PropertyModel; }
			set { base.Data = value; }
		}

		#endregion

		#region ----------------COMMANDS----------------

		#region generic command
		private ICommand genericCommand;
		public ICommand GenericCommand
		{
			get
			{
				if (genericCommand == null)
					genericCommand = new RelayCommand<string>(
						delegate(string param)
						{
							Messenger.Default.Send<PropertyModel>(Data, param);
						},
						delegate(string param)
						{
							return Data != null;
						});
				return genericCommand;
			}
		}

		#endregion

		#endregion
	}
}
