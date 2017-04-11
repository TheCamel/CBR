using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Xml.Linq;
using CBR.Core.Helpers;

namespace CBR.Core.Formats.ePUB
{
	public class ePUBGuideItem
	{
		#region -----------------CONSTRUCTOR-----------------

		/// <summary>
		/// Constructor
		/// </summary>
		public ePUBGuideItem()
		{
		}

		#endregion

		#region -----------------PROPERTIES-----------------

		[Browsable(true)]
		[Description("Gets the id attribute of the current item node")]
		public string hRef { get; set; }

		[Browsable(true)]
		[Description("Gets the linear attribute of the current item node")]
		public string Type { get; set; }

		[Browsable(true)]
		[Description("Gets the linear attribute of the current item node")]
		public string Title { get; set; }

		#endregion

		#region -----------------METHODS-----------------

		internal XElement ToElement()
		{
			XElement xElement = new XElement(ePUBHelper.XmlNamespaces.Opf + "reference", new object[]
			{
				new XAttribute("href", hRef),
				new XAttribute("type", Type)
			});

			if (!string.IsNullOrEmpty(Title))
			{
				xElement.SetAttributeValue("title", Title);
			}
			
			return xElement;
		}
		#endregion
	}
}
