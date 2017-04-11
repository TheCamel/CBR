using CBR.Core.Models;

namespace CBR.Core.Services
{
	/// <summary>
	/// Internal class for thread data exchanges
	/// </summary>
	internal class ThreadExchangeData
	{
		/// <summary>
		/// Current catalog data
		/// </summary>
		public Catalog ThreadCatalog;
		/// <summary>
		/// Current book data
		/// </summary>
		public Book ThreadBook;
		/// <summary>
		/// Book file to read
		/// </summary>
		public string BookPath;
	}
}
