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
	public class ePUBGuide
	{
		#region -----------------CONSTRUCTOR-----------------

		/// <summary>
		/// Constructor
		/// </summary>
		public ePUBGuide()
		{
			Items = new List<ePUBGuideItem>();
		}

		#endregion

		#region -----------------PROPERTIES-----------------

		[Browsable(true)]
		[Description("ItemsRef")]
		public List<ePUBGuideItem> Items { get; set; }

		#endregion

		#region -----------------METHODS-----------------

		public bool AddDocument(string idref)
		{
			return true;
		}


		internal XElement ToElement()
		{
			XElement xElement = new XElement(ePUBHelper.XmlNamespaces.Opf + "guide");

			foreach (ePUBGuideItem current in Items)
			{
				xElement.Add(current.ToElement());
			}
			return xElement;
		}
		#endregion
	}
}
