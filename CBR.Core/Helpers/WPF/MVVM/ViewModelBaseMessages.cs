using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBR.Core.Helpers
{
	public class ViewModelBaseMessages
	{
		/// <summary>
		/// from explorer and home to main
		/// </summary>
		public const string ContextCommand = "ContextCommand";
		/// <summary>
		/// Active document changed
		/// </summary>
		public const string DocumentRequestClose = "DocumentRequestClose";
		/// <summary>
		/// Active document changed
		/// </summary>
		public const string DocumentActivChanged = "DocumentActivChanged";
		/// <summary>
		/// 
		/// </summary>
		public const string MenuItemCommand = "MenuItemCommand";

		/// <summary>
		/// when a book or catalog file is added/removed, notify the recent file backstage panel
		/// </summary>
		public const string RecentListChanged = "RecentListChanged";

		/// <summary>
		/// when a book or catalog file is changed, notify the recent file backstage panel
		/// </summary>
		public const string RecentFileChanged = "RecentFileChanged";

		/// <summary>
		/// when catalog list is changed
		/// </summary>
		public const string CatalogListItemChanged = "CatalogListItemChanged";

		/// <summary>
		/// when catalog list is changed
		/// </summary>
		public const string CatalogListItemAdded = "CatalogListItemAdded";

		/// <summary>
		/// when catalog list is changed
		/// </summary>
		public const string CatalogListItemRemoved = "CatalogListItemRemoved";


		/// <summary>
		/// when catalog list is changed
		/// </summary>
		public const string CatalogRefreshCover = "CatalogRefreshCover";
	}
}
