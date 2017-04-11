using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CBR.Core.Models
{
	public class ExtendedInfo
	{
		#region ----------------DEFAULTs----------------
        /// <summary>
        /// Construtor
        /// </summary>
		public ExtendedInfo()
        {
			// display options
			ShowFullScreenOptimized = true;
			ShowZoomFlyer = true;
			ShowTooltipExplorer = false;
			ShowGridCover = false;
			DefaultExplorerView = "ExplorerGridView";

			// behave options
			BehavePageTempo = true;

			//proxy settings
			Proxy = new ProxyInfo();

            SharePort = 9999;
            ShareAdress = "localhost";
            ShareOnStartup = false;
        }
		#endregion

		#region ----------------display options----------------

		/// <summary>
		/// full sreen display nothing
		/// </summary>
		public bool ShowFullScreenOptimized { get; set; }

		/// <summary>
		/// show zoom control on the document
		/// </summary>
		public bool ShowZoomFlyer { get; set; }

		/// <summary>
		/// Show tooltip in catalog exeplorer
		/// </summary>
		public bool ShowTooltipExplorer { get; set; }

		/// <summary>
		/// Show cover in grid mode
		/// </summary>
		public bool ShowGridCover { get; set; }

		/// <summary>
		/// default explorer view :simple, extended, grid
		/// </summary>
		public string DefaultExplorerView { get; set; }

		#endregion

		#region ----------------behave options----------------

		/// <summary>
		/// use tempo on mouse at top/bottom pages
		/// </summary>
		public bool BehavePageTempo { get; set; }
		
		#endregion

		#region ----------------proxy options----------------
		/// <summary>
		/// proxy informations
		/// </summary>
		public ProxyInfo Proxy { get; set; }

		#endregion

        #region ----------------share options----------------

        /// <summary>
        /// wcf server name
        /// </summary>
        public string ShareAdress { get; set; }

        /// <summary>
        /// wcf server port
        /// </summary>
        public int SharePort { get; set; }

        /// <summary>
        /// start the server asap
        /// </summary>
        public bool ShareOnStartup { get; set; }

        #endregion
	}
}
