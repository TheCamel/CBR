using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CBR.Core.Helpers;
using System.Windows.Media.Imaging;
using CBR.Core.Files;
using CBR.Core.Helpers.NET.Properties;

namespace CBR.Core.Formats.OPDS
{
	public class OpdsItem : OpdsItemBase
	{
		/// <summary>
		/// 
		/// </summary>
		[UserPropertyAttribute(false, true, "OPDS.Properties.AuthorName")]
		public string AuthorName { get; set; }
		
		/// <summary>
		/// 
		/// </summary>
		public Uri AuthorWeb { get; set; }
		
		/// <summary>
		/// 
		/// </summary>
		[UserPropertyAttribute(false, true, "OPDS.Properties.Language")]
		public string Language { get; set; }

		public List<string> Categories { get; set; }
		public string CategoriesLabel { get { return string.Join(", ", Categories.ToArray()); } }

		public Uri Icon { get; set; }

		public Uri AlternateUrl { get; set; }
		public Uri SameAuthorUrl { get; set; }
		public Uri ThumbnailUrl { get; set; }
		
		public List<OpdsDownload> Downloads { get; set; }
	}
}
