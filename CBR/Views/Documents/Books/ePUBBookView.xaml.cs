using System.Windows;
using System.Windows.Controls;
using CBR.Core.Formats.ePUB;
using CBR.ViewModels;
using CBR.Core.Helpers;
using System;
using GalaSoft.MvvmLight.Messaging;

namespace CBR.Views
{
    /// <summary>
    /// Interaction logic for ePUBBookView.xaml
    /// </summary>
    public partial class ePUBBookView : UserControl
    {
        public ePUBBookView()
        {
			using (new TimeLogger("ePUBBookView.ePUBBookView"))
			{
				InitializeComponent();
                VisualHelper.AllowFocus(this);
			}
        }

		private void UserControl_Loaded(object sender, RoutedEventArgs e)
		{
			Messenger.Default.Register<PropertyChangedMessage<Uri>>(this, HandleSourceChange);
		}

		private void UserControl_Unloaded(object sender, RoutedEventArgs e)
		{
			Messenger.Default.Unregister<PropertyChangedMessage<Uri>>(this, HandleSourceChange);
		}

		private void HandleSourceChange( PropertyChangedMessage<Uri> action )
		{
			this.contentWebBrowser.Source = (action.NewValue);
		}
    }
}
