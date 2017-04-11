using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CBR.Core.Helpers.NET.Properties;

namespace CBR.Core.Formats.OPDS
{
	public class OpdsItemBase
	{
		/// <summary>
		/// Item title
		/// </summary>
		[UserPropertyAttribute(false, true, "OPDS.Properties.Title")]
		public string Title { get; set; }

		/// <summary>
		/// Last update date
		/// </summary>
		[UserPropertyAttribute(false, true, "OPDS.Properties.UpdatedDate")]
		public DateTime Updated { get; set; }

		/// <summary>
		/// Content
		/// </summary>
		public string Content { get; set; }

	}
}
