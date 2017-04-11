using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CBR.Core.Helpers;
using CBR.Components.Controls;

namespace CBR.ViewModels
{
    public class ViewModelMessages : ViewModelBaseMessages
    {
        
        /// <summary>
        /// when settings are changed
        /// </summary>
        public const string SettingsChanged = "SettingsChanged";

        /// <summary>/// when extended settings are changed
        
        /// </summary>
        public const string ExtendedSettingsChanged = "ExtendedSettingsChanged";        
        
		/// <summary>
        /// from mainview to explorer
        /// </summary>
        public const string CatalogChanged = "CatalogChanged";

        /// <summary>
        /// Update the catalog with new books
        /// </summary>
		public const string CatalogUpdate = "CatalogUpdate";

        /// <summary>
        /// when selection change in the explorer view
        /// </summary>
        public const string BookSelected = "BookSelected";

        /// <summary>
        /// from main ui to update the explorer
        /// </summary>
		public const string ExplorerViewModeChanged = "ExplorerViewModeChanged";

        /// <summary>
        /// from explorer menu item to explorer to change group properties
        /// </summary>
		public const string ExplorerGroupChanged = "ExplorerGroupChanged";

        /// <summary>
        /// from explorer menu item to explorer to change sort properties
        /// </summary>
		public const string ExplorerSortChanged = "ExplorerSortChanged";

        /// <summary>
        /// Device is added, update the disk list in the device view model
        /// </summary>
        public const string DeviceAdded = "DeviceAdded";

        /// <summary>
        /// Device is removed, update the disk list in the device view model
        /// </summary>
        public const string DeviceRemoved = "DeviceRemoved";

        /// <summary>
        /// new selection in the device tree view, update the device content view
        /// </summary>
        public const string DeviceContentChanged = "DeviceContentChanged";

		/// <summary>
		/// 
		/// </summary>
		public const string SwapTwoPageView = "SwapTwoPageView";

		/// <summary>
		/// 
		/// </summary>
		public const string LanguagesChanged = "LanguagesChanged";

		/// <summary>
		/// 
		/// </summary>
		public const string RssSortChanged = "RssSortChanged";


		public const string TocContentChanged = "TocContentChanged";

		public const string TocIndexChanged = "TocIndexChanged";

        public const string TocNaviguateTo = "TocNaviguateTo";


    }
}
