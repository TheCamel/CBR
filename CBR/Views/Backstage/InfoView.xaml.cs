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
using CBR.ViewModels;
using CBR.Core.Helpers;

namespace CBR.Views
{
    /// <summary>
    /// Interaction logic for InfoView.xaml
    /// </summary>
    public partial class InfoView : UserControl
    {
        public InfoView()
        {
			using (new TimeLogger("InfoView.InfoView"))
			{
				InitializeComponent();

				this.DataContext = new InfoViewModel(null);
			}
        }
    }
}
