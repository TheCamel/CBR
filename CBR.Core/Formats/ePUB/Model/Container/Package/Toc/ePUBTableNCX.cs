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
	public class ePUBTableNCX
	{
		#region -----------------CONSTRUCTOR-----------------

		/// <summary>
		/// Constructor
		/// </summary>
		public ePUBTableNCX()
		{
			Items = new List<ePUBNavPoint>();
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="filePath"></param>
		public ePUBTableNCX(string tocPath)
		{
		}

		#endregion

		#region -----------------PROPERTIES-----------------

		[Browsable(true)]
		[Description("Get or set the publication author in the docAuthor/text element of the NCX document")]
		public string Author { get; set; }

		[Browsable(true)]
		[Description("Get or set the dtb:uid meta element in the head element of the NCX")]
		public string Identifier { get; set; }

		[Browsable(true)]
		[Description("Get the navMap node of the NCX and return it as an XML document")]
		public List<ePUBNavPoint> Items { get; set; }

		[Browsable(true)]
		[Description("Get or set the publication title in the docTitle/text element of the NCX document")]
		public string Title { get; set; }

		#endregion

		#region -----------------METHODS-----------------

		public void AddDocument(
	string id,
	string text,
	string src
)
		{
		}
public void InsertAfter(
	string id,
	string newId,
	string text,
	string src
)
	{
}
public void InsertBefore(
	string id,
	string newId,
	string text,
	string src
)
	{
}
public void MoveDown(
	string id
)
	{
}
public void MoveUp(
	string id
)
	{
}
public void Remove(
	string id
)
	{
}
public void Rename(
	string id,
	string newName
)
	{
}
public void Save()
{
}
		internal XDocument ToXml()
		{
			XDocument xDocument = new XDocument(new object[]
			{
				new XDocumentType("ncx", "-//NISO//DTD ncx 2005-1//EN", "http://www.daisy.org/z3986/2005/ncx-2005-1.dtd", null)
			});

			XElement xElement = new XElement(ePUBHelper.XmlNamespaces.NcxToc + "ncx");
			xElement.Add(this.CreateHeadElement());

			xElement.Add(new XElement(ePUBHelper.XmlNamespaces.NcxToc + "docTitle", 
				new XElement(ePUBHelper.XmlNamespaces.NcxToc + "text", this.Title)));
			
			//foreach (string current in this._authors)
			//{
			//    xElement.Add(new XElement(ePUBHelper.XmlNamespaces.NcxToc + "docAuthor", new XElement(NCX.NcxNS + "text", current)));
			//}

			XElement xElement2 = new XElement(ePUBHelper.XmlNamespaces.NcxToc + "navMap");
			foreach (ePUBNavPoint current2 in this.Items)
			{
				xElement2.Add(current2.ToElement());
			}
			xElement.Add(xElement2);

			xDocument.Add(xElement);
			
			return xDocument;
		}

		private XElement CreateHeadElement()
		{
			XElement xElement = new XElement(ePUBHelper.XmlNamespaces.NcxToc + "head");
			xElement.Add(new XElement(ePUBHelper.XmlNamespaces.NcxToc + "meta", new object[]
			{
				new XAttribute("name", "dtb:uid"),
				new XAttribute("content", this.Identifier)
			}));
			xElement.Add(new XElement(ePUBHelper.XmlNamespaces.NcxToc + "meta", new object[]
			{
				new XAttribute("name", "dtb:depth"),
				new XAttribute("content", "1")
			}));
			xElement.Add(new XElement(ePUBHelper.XmlNamespaces.NcxToc + "meta", new object[]
			{
				new XAttribute("name", "dtb:totalPageCount"),
				new XAttribute("content", "0")
			}));
			xElement.Add(new XElement(ePUBHelper.XmlNamespaces.NcxToc + "meta", new object[]
			{
				new XAttribute("name", "dtb:maxPageNumber"),
				new XAttribute("content", "0")
			}));
			return xElement;
		}

		#endregion
	}
}
