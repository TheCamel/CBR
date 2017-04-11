using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using CBR.Core.Helpers;
using CBR.Core.Helpers.Localization;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;

namespace CBR.ViewModels.Others
{
	/// <summary>
	/// ViewModel for the Localize screen
	/// </summary>
	public class LocalizeViewModel : ViewModelBaseExtended
	{
		#region ----------------PROPERTIES----------------

		private ICollectionView _Languages = null;
		/// <summary>
		/// language items collection
		/// </summary>
		public ICollectionView Languages
		{
			get
			{
				if (_Languages == null)
				{
					_Languages = CollectionViewSource.GetDefaultView(CultureManager.Instance.GetAvailableCultures());
					_Languages.CurrentChanged += new EventHandler(_Languages_CurrentChanged);
				}

				return _Languages;
			}
			set
			{
				if (_Languages != value)
				{
					_Languages.CurrentChanged -= new EventHandler(_Languages_CurrentChanged);

					_Languages = value;
					RaisePropertyChanged("Languages");
				}
			}
		}

		/// <summary>
		/// if language change, need to update the module and resource items lists
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void _Languages_CurrentChanged(object sender, EventArgs e)
		{
			//Modules = null;
			ResourceItems = null;
		}

		/// <summary>
		/// shortcut to the 2 letter language code
		/// </summary>
		/// <returns></returns>
		public string ActualCode
		{
			get { return (Languages.CurrentItem as CultureInfo).IetfLanguageTag; }
		}

		private ICollectionView _Modules = null;
		/// <summary>
		/// modules items collection
		/// </summary>
		public ICollectionView Modules
		{
			get
			{
				if (_Modules == null)
				{
					_Modules = CollectionViewSource.GetDefaultView(
						CultureManager.Instance.GetAvailableModules(ActualCode));

					_Modules.CurrentChanged += new EventHandler(_Modules_CurrentChanged);
				}
				return _Modules;
			}
			set
			{
				if (_Modules != value)
				{
					_Modules.CurrentChanged -= new EventHandler(_Modules_CurrentChanged);

					_Modules = value;
					RaisePropertyChanged("Modules");
				}
			}
		}

		/// <summary>
		/// if modul change, update the resources
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void _Modules_CurrentChanged(object sender, EventArgs e)
		{
			ResourceItems = null;
		}

		/// <summary>
		/// shortcut to the selected modul
		/// </summary>
		/// <returns></returns>
		private string ActualModul()
		{
			return Modules.CurrentItem as string;
		}

		private ICollectionView _Cultures = null;
		/// <summary>
		/// available culture items collection not allready created
		/// </summary>
		public ICollectionView Cultures
		{
			get
			{
				if (_Cultures == null)
				{
					List<CultureInfo> list = CultureInfo.GetCultures(CultureTypes.AllCultures).ToList();

					List<CultureInfo> remList = new List<CultureInfo>();
					foreach( CultureInfo inf in CultureManager.Instance.GetAvailableCultures())
						remList.Add(inf);

					foreach (CultureInfo inf in remList)
					{
						CultureInfo fo = list.Find(p => p.IetfLanguageTag == inf.IetfLanguageTag);
						list.Remove(fo);
					}
					list.GroupBy(p => p.IetfLanguageTag);

					_Cultures = CollectionViewSource.GetDefaultView(list);
				}

				return _Cultures;
			}
			set
			{
				if (_Cultures != value)
				{
					_Cultures = value;
					RaisePropertyChanged("Cultures");
				}
			}
		}

		private ICollectionView _ResourceItems = null;
		/// <summary>
		/// resource items collection
		/// </summary>
		public ICollectionView ResourceItems
		{
			get
			{
				if (_ResourceItems == null)
				{
					_ResourceItems = CollectionViewSource.GetDefaultView(
						CultureManager.Instance.GetModuleResource(ActualCode, ActualModul()));

					_ResourceItems.SortDescriptions.Add(new SortDescription("Key", ListSortDirection.Descending));
				}

				return _ResourceItems;
			}
			set
			{
				if (_ResourceItems != value)
				{
					_ResourceItems = value;
					RaisePropertyChanged("ResourceItems");
				}
			}
		}

		#endregion

		#region ---------------- COMMANDS ----------------

		#region select command
		private ICommand selectCommand;
		public ICommand SelectCommand
		{
			get
			{
				if (selectCommand == null)
					selectCommand = new RelayCommand<string>(
						delegate(string param)
						{
							CultureManager.Instance.UICulture = Languages.CurrentItem as CultureInfo;
						},
						delegate(string param)
						{
							return true;
						});
				return selectCommand;
			}
		}
		#endregion

		#region refresh command
		private ICommand refreshCommand;
		public ICommand RefreshCommand
		{
			get
			{
				if (refreshCommand == null)
					refreshCommand = new RelayCommand<string>(
						delegate(string param)
						{
							CultureManager.Instance.Refresh();
						},
						delegate(string param)
						{
							return true;
						});
				return refreshCommand;
			}
		}
		#endregion

		#region reload command
		private ICommand reloadCommand;
		public ICommand ReloadCommand
		{
			get
			{
				if (reloadCommand == null)
					reloadCommand = new RelayCommand<string>(
						delegate(string param)
						{
							ResourceItems = null;
							ResourceItems.Refresh();
						},
						delegate(string param)
						{
							return true;
						});
				return reloadCommand;
			}
		}
		#endregion

		#region create command
		private ICommand createCommand;
		public ICommand CreateCommand
		{
			get
			{
				if (createCommand == null)
					createCommand = new RelayCommand<string>(
						delegate(string param)
						{
							CultureManager.Instance.CreateCulture(Cultures.CurrentItem as CultureInfo);
							_Languages = null;
							_Modules = null;
							_ResourceItems = null;
							_Cultures = null;

							RaisePropertyChanged("Languages");
							RaisePropertyChanged("Modules");
							RaisePropertyChanged("ResourceItems");
							RaisePropertyChanged("Cultures");

							Messenger.Default.Send<CultureInfo>(null, ViewModelMessages.LanguagesChanged);
						},
						delegate(string param)
						{
							return true;
						});
				return createCommand;
			}
		}
		#endregion

		#region save command
		private ICommand saveCommand;
		public ICommand SaveCommand
		{
			get
			{
				if (saveCommand == null)
					saveCommand = new RelayCommand<string>(
						delegate(string param)
						{
							CultureManager.Instance.SaveResources();
						},
						delegate(string param)
						{
							return true;
						});
				return saveCommand;
			}
		}
		#endregion

		#region delete command
		private ICommand deleteCommand;
		public ICommand DeleteCommand
		{
			get
			{
				if (deleteCommand == null)
					deleteCommand = new RelayCommand<LocalizationItem>(
						delegate(LocalizationItem param)
						{
							CultureManager.Instance.DeleteCulture(ActualCode);
							_Languages = null;
							_Modules = null;
							_ResourceItems = null;
							_Cultures = null;

							RaisePropertyChanged("Languages");
							RaisePropertyChanged("Modules");
							RaisePropertyChanged("ResourceItems");
							RaisePropertyChanged("Cultures");

							Messenger.Default.Send<CultureInfo>(null, ViewModelMessages.LanguagesChanged);
						},
						delegate(LocalizationItem param)
						{
							return true;
						});
				return deleteCommand;
			}
		}
		#endregion

		#region delete item command
		private ICommand deleteItemCommand;
		public ICommand DeleteItemCommand
		{
			get
			{
				if (deleteItemCommand == null)
					deleteItemCommand = new RelayCommand<LocalizationItem>(
						delegate(LocalizationItem param)
						{
							CultureManager.Instance.DeleteResource(ActualCode, ActualModul(), param);
							ResourceItems = null;
							ResourceItems.Refresh();
						},
						delegate(LocalizationItem param)
						{
							return true;
						});
				return deleteItemCommand;
			}
		}
		#endregion

		#endregion

	}
}
