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
using System.Windows.Navigation;
using System.Windows.Shapes;
using CBR.ViewModels;
using CBR.Core.Helpers;
using GalaSoft.MvvmLight.Messaging;
using CBR.ViewModels.Messages;

namespace CBR.Views
{
	/// <summary>
	/// Interaction logic for TocView.xaml
	/// </summary>
	public partial class TocView : UserControl
	{
		public TocView()
		{
			using( new TimeLogger("TocView.TocView") )
			{
				InitializeComponent();
                VisualHelper.AllowFocus(this);
			} 
		}

		private void TocTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
		{
            Messenger.Default.Send<TocNaviguateNotification>(new TocNaviguateNotification( TocTree.SelectedItem ));
		}
	}
}
