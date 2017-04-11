using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using CBR.Core.Files.Conversion;
using CBR.Core.Helpers;

namespace CBR.Core.Models
{
    /// <summary>
    /// All programm settings that are serialized in the properties of the programm
    /// </summary>
    public class WorkspaceInfo
    {
        #region ----------------DEFAULTs----------------
        /// <summary>
        /// Construtor
        /// </summary>
        public WorkspaceInfo()
        {
            AutoFitMode = 0;
            ImageCacheCount = 6;
            ImageCacheDuration = 40;
			MagnifierScaleFactor = 200;
			MagnifierSize = 1;
            MaxRecentFile = 10;
            RecentCatalogList = new List<RecentFileInfo>();
            RecentFileList = new List<RecentFileInfo>();
            Dynamics = new List<string>();
            ConvertParameters = new ContractParameters();
            DeviceInfoList = new List<DeviceInfo>();
            StartingLanguageCode = "en";

			Extended = new ExtendedInfo();
			Feed = new FeedInfo();
        }
        #endregion

        #region ----------------PROPERTIES----------------

        public int AutoFitMode { get; set; }
        public int ImageCacheDuration { get; set; }
        public int ImageCacheCount { get; set; }

        public double MagnifierScaleFactor { get; set; }
        public double MagnifierSize { get; set; }

        public int MaxRecentFile { get; set; }

        public List<RecentFileInfo> RecentFileList { get; set; }
        public List<RecentFileInfo> RecentCatalogList { get; set; }

        public List<string> Dynamics { get; set; }

        public ContractParameters ConvertParameters { get; set; }
        public List<DeviceInfo> DeviceInfoList { get; set; }

        public string StartingLanguageCode { get; set; }

		/// <summary>
		/// extended options
		/// </summary>
		public ExtendedInfo Extended { get; set; }

		/// <summary>
		/// opds feeds
		/// </summary>
		public FeedInfo Feed { get; set; }

		#endregion
    }
}
