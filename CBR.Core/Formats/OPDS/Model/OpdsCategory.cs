using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CBR.Core.Helpers;
using System.Windows.Media.Imaging;

namespace CBR.Core.Formats.OPDS
{
	public class OpdsCategory : OpdsItemBase
	{
		public BitmapImage Thumbnail { get; set; }
		public Uri Link { get; set; }
	}
}
