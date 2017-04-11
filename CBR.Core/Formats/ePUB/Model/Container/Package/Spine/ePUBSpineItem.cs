using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Xml.Linq;
using CBR.Core.Helpers;

namespace CBR.Core.Formats.ePUB
{
	public class ePUBSpineItem
	{
		#region -----------------CONSTRUCTOR-----------------

		/// <summary>
		/// Constructor
		/// </summary>
		public ePUBSpineItem()
		{
		}

		/// <summary>
		/// Create an instance using the given packageDocument XmlDocument. The metadata node is extracted 
		/// </summary>
		/// <param name="filePath"></param>
		public ePUBSpineItem(string id, string linear)
		{
			Id = id;
			Linear = linear == "no" ? false : true;
		}

		#endregion

		#region -----------------PROPERTIES-----------------

		[Browsable(true)]
		[Description("Gets the id attribute of the current item node")]
		public string Id { get; set; }

		[Browsable(true)]
		[Description("Gets the linear attribute of the current item node")]
		public bool Linear { get; set; }

		#endregion

		#region -----------------METHODS-----------------

		internal XElement ToElement()
		{
			XElement xElement = new XElement(ePUBHelper.XmlNamespaces.Opf + "itemref", new XAttribute("idref", Id));
			if (!Linear)
			{
				xElement.SetAttributeValue("linear", "no");
			}
			return xElement;
		}
		#endregion
	}
}
