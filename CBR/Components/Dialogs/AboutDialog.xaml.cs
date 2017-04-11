using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Reflection;

namespace CBR.Components.Dialogs
{
	/// <summary>
	/// Interaction logic for AboutDialog.xaml
	/// </summary>
	public partial class AboutDialog : Window
	{
		public AboutDialog()
		{
			InitializeComponent();
		}

		#region --------------------FUNCTIONS--------------------

		/// <summary>
		/// Fill the assemblies
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			Assembly EntryAssembly = Assembly.GetEntryAssembly();

			// fill the executable version
			this.labelApplicationVersion.Content = EntryAssembly.GetName().Version.ToString();

			//fill with the dependencies
			this.listBoxAssembliesList.Items.Clear();

			foreach (AssemblyName assembly in EntryAssembly.GetReferencedAssemblies())
			{
				this.listBoxAssembliesList.Items.Add(assembly.Name + ", Version=" + assembly.Version);
			}
		}
		#endregion
	}
}
