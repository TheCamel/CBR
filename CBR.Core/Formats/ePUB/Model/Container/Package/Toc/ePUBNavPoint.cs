using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Xml.Linq;
using CBR.Core.Helpers;

namespace CBR.Core.Formats.ePUB
{
	public class ePUBNavPoint
	{
		#region -----------------CONSTRUCTOR-----------------

		/// <summary>
		/// Constructor
		/// </summary>
		public ePUBNavPoint()
		{
			Items = new List<ePUBNavPoint>();
		}

        #endregion

        #region -----------------PROPERTIES-----------------

        public string PageSource { get; set; }
        public string TargetId { get; set; }
        public string XamlId{ get; set; }


        [Browsable(true)]
		[Description("Gets the id attribute of the current item node")]
		public string Id { get; set; }

		[Browsable(true)]
		[Description("Gets the linear attribute of the current item node")]
		public int PlayOrder { get; set; }

		[Browsable(true)]
		[Description("Gets the id attribute of the current item node")]
		public string Label { get; set; }

		[Browsable(true)]
		[Description("Gets the id attribute of the current item node")]
		public string Content { get; set; }

		[Browsable(true)]
		[Description("Gets the id attribute of the current item node")]
		public List<ePUBNavPoint> Items { get; set; }

		#endregion

		#region -----------------METHODS-----------------

		internal XElement ToElement()
		{
			XElement xElement = new XElement(ePUBHelper.XmlNamespaces.NcxToc + "navPoint", new object[]
			{
				new XAttribute("id", this.Id),
				new XAttribute("playOrder", this.PlayOrder)
			});

			xElement.Add(new XElement(ePUBHelper.XmlNamespaces.NcxToc + "navLabel", new XElement(ePUBHelper.XmlNamespaces.NcxToc + "text", this.Label)));
			xElement.Add(new XElement(ePUBHelper.XmlNamespaces.NcxToc + "content", new XAttribute("src", this.Content)));

			foreach (ePUBNavPoint current in this.Items)
			{
				xElement.Add(current.ToElement());
			}
			return xElement;
		}

		#endregion
	}
}
