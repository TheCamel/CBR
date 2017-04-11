using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using CBR.Core.Helpers;
using System.Windows.Media.Imaging;

namespace CBR.Core.Formats.OPDS
{
	public class OpdsFeed
	{
		public string Author { get; set; }
		public string PageTitle { get; set; }
		public Uri OpdsUrl { get; set; }
		
		public Uri WebUrl { get; set; }
		public Uri Icon { get; set; }

		//public Uri StartUrl { get; set; }
		public Uri SearchUrl { get; set; }
		public Uri NextUrl { get; set; }
		public Uri PreviousUrl { get; set; }

		public int TotalResults { get; set; }
		public int ItemPerPage { get; set; }

		public List<OpdsItemBase> Items { get; set; }
	}
}
