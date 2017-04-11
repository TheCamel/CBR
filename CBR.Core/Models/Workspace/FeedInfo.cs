using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace CBR.Core.Models
{
	public class FeedInfo
	{
		#region ----------------DEFAULTs----------------
		
		public FeedInfo()
        {
			Feeds = new List<FeedItemInfo>();
			DownloadFolder = string.Empty;
			UpdateCatalog = true;
			AutomaticOpen = false;
			CacheDuration = 1;
        }
		
		#endregion

		/// <summary>
		/// opds feeds list
		/// </summary>
		public List<FeedItemInfo> Feeds { get; set; }

		/// <summary>
		/// if not empty, else ask for folder for each download
		/// </summary>
		public string DownloadFolder { get; set; }

		/// <summary>
		/// update the catalog automatically
		/// </summary>
		public bool UpdateCatalog { get; set; }

		/// <summary>
		/// Automatically open a downloaded item
		/// </summary>
		public bool AutomaticOpen { get; set; }

		/// <summary>
		/// Cache duration in days
		/// </summary>
		public int CacheDuration { get; set; }
	}


	[Serializable]
	public class FeedItemInfo
	{
		public FeedItemInfo()
        {
        }

		public FeedItemInfo(string name, string url, string IetfLanguageTag)
        {
            Name = name;
            Url= url;
			Language = IetfLanguageTag;
        }

		[XmlAttribute]
		public string Name { get; set; }
		
		[XmlAttribute]
		public string Url { get; set; }
		
		[XmlAttribute]
		public string Language{ get; set; }
	}
}
