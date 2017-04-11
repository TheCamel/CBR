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
	public class ePUBSpine
	{
		#region -----------------CONSTRUCTOR-----------------

		/// <summary>
		/// Constructor
		/// </summary>
		public ePUBSpine()
		{
			Items = new List<ePUBSpineItem>();
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="filePath"></param>
		public ePUBSpine(XmlDocument packageDocument)
		{
			Items = new List<ePUBSpineItem>();
		}

		#endregion

		#region -----------------PROPERTIES-----------------

		[Browsable(true)]
		[Description("TableOfContentId")]
		public string TableOfContentId { get; set; }

		[Browsable(true)]
		[Description("ItemsRef")]
		public List<ePUBSpineItem> Items { get; set; }

		#endregion

		#region -----------------METHODS-----------------

		public bool AddDocument(string idref)
		{

			return true;
		}


		internal XElement ToElement()
		{
			XElement xElement = new XElement(ePUBHelper.XmlNamespaces.Opf + "spine");

			if (!string.IsNullOrEmpty(TableOfContentId))
			{
				xElement.Add(new XAttribute("toc", TableOfContentId));
			}

			foreach (ePUBSpineItem current in Items)
			{
				xElement.Add(current.ToElement());
			}
			return xElement;
		}
		#endregion
	}
}
