using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Xml;

namespace CBR.Core.Formats.ePUB
{
	public class ePUBManifest
	{
		#region -----------------CONSTRUCTOR-----------------

		/// <summary>
		/// Constructor
		/// </summary>
		public ePUBManifest()
		{
			Items = new List<ePUBManifestItem>();
		}

		/// <summary>
		/// Create an instance using the given packageDocument XmlDocument. The metadata node is extracted 
		/// </summary>
		/// <param name="filePath"></param>
		public ePUBManifest(XmlNode node)
		{
			Items = new List<ePUBManifestItem>();
		}

		#endregion

		#region -----------------PROPERTIES-----------------

		[Browsable(true)]
		[Description("Gets the manifest items")]
		public List<ePUBManifestItem> Items { get; set; }

		#endregion

		#region -----------------METHODS-----------------

		public ePUBManifestItem GetManifestItemById(string id)
		{
			return new ePUBManifestItem();
		}

		public void AddDocument(
	string id,
	string href,
	string media_type
)
		{
		}
		#endregion
	}
}
