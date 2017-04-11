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
using CBR.Core.Helpers;

namespace CBR.Views
{
    /// <summary>
    /// Interaction logic for XpsBookView.xaml
    /// </summary>
    public partial class XpsBookView : UserControl
    {
        public XpsBookView()
        {
			using (new TimeLogger("XpsBookView.XpsBookView"))
			{
				InitializeComponent();
                VisualHelper.AllowFocus(this);
			}
        }

		public static readonly DependencyProperty ThisProperty =
			DependencyProperty.Register("This", typeof(XpsBookView), typeof(XpsBookView));

		public XpsBookView This
		{
			get { return GetValue(ThisProperty) as XpsBookView; }
			set { /* do nothing */ }
		}

		private void UserControl_Loaded(object sender, RoutedEventArgs e)
		{
			SetValue(ThisProperty, this);
		}  
    }
}
