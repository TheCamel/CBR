using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CBR.Core.Files;
using CBR.Core.Models;

namespace CBR.Core.Formats.OPDS
{
	public class OpdsDownload 
	{
		/// <summary>
		/// associated internal document type
		/// </summary>
		public DocumentType Type { get; set; }
		
		/// <summary>
		/// link to the file resource
		/// </summary>
		public Uri Link { get; set; }

		/// <summary>
		/// we repeat opdsitem title here for save purpose
		/// </summary>
		public string Title { get; set; }
	}
}
