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
using CBR.Core.Files.Publisher;
using System.Collections;
using CBR.ViewModels;
using CBR.Core.Models;

namespace CBR.Views
{
	/// <summary>
	/// Interaction logic for PublishView.xaml
	/// </summary>
	public partial class PublishView : UserControl
	{
		public PublishView()
		{
			InitializeComponent();
		}

		private void btnPublish_Click(object sender, RoutedEventArgs e)
		{
			CollectionPublisher cp = new CollectionPublisher();
			cp.Columns = new List<string>() { "Folder", "FileName", "FilePath", "PageCount", "Size", "Rating" };
			cp.GroupBy = "Folder";
			cp.SortBy = "FileName";
			cp.FileOutput = "d:\\test.html";
			cp.DataCollection = ((this.DataContext as MainViewModel).Data as Catalog).Books as ICollection;
			cp.Publish(@"D:\PROJECTS\CBR\CBR\bin\x86\Debug\Templates\HTMLPage1.htm");

		}
	}
}
