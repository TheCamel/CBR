using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Xml;
using System.Xml.Linq;
using CBR.Core.Helpers;

namespace CBR.Core.Formats.ePUB
{
	public class ePUBManifestItem
	{
		#region -----------------CONSTRUCTOR-----------------

		/// <summary>
		/// Constructor
		/// </summary>
		public ePUBManifestItem()
		{
		}

		/// <summary>
		/// Create an instance using the given packageDocument XmlDocument. The metadata node is extracted 
		/// </summary>
		/// <param name="filePath"></param>
		public ePUBManifestItem(XmlNode node)
		{
		}

        #endregion

        #region -----------------PROPERTIES-----------------

        public string XamlId{ get; set; }

        [Browsable(true)]
		[Description("Gets the href attribute of the current item node")]
		public string hRef { get; set; }
        public string hRefForPath { get; set; }

        [Browsable(true)]
		[Description("Gets the id attribute of the current item node")]
		public string Id { get; set; }

		[Browsable(true)]
		[Description("Gets the mediatype attribute of the current item node")]
		public string MediaType { get; set; }


		[Browsable(false)]
		[Description("Gets the fallback attribute of the current item node")]
		public string Fallback { get; set; }

		[Browsable(false)]
		[Description("Gets the fallbackstyle attribute of the current item node")]
		public string FallbackStyle { get; set; }

		[Browsable(false)]
		[Description("Gets the requiredmodules attribute of the current item node")]
		public string RequiredModules { get; set; }

		[Browsable(false)]
		[Description("Gets the requirednamespaced attribute of the current item node")]
		public string RequiredNamespaced { get; set; }

		#endregion

		#region -----------------METHODS-----------------

		internal XElement ToElement()
		{
			XElement xElement = new XElement(ePUBHelper.XmlNamespaces.Opf + "item");
			xElement.SetAttributeValue("id", Id);
			xElement.SetAttributeValue("href", hRef);
			xElement.SetAttributeValue("media-type", MediaType);
			return xElement;
		}
		#endregion
	}
}
