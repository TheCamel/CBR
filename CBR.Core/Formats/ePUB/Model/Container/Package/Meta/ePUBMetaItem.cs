using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using CBR.Core.Helpers;

namespace CBR.Core.Formats.ePUB
{
	public class ePUBMetaItem
	{
		public string Name { get; set; }
		public string Value { get; set; }

		internal ePUBMetaItem(string name, string value)
		{
			this.Name = name;
			this.Value = value;
		}

		internal XElement ToElement()
		{
			XElement xElement = new XElement("meta");
			xElement.SetAttributeValue("name", this.Name);
			xElement.SetAttributeValue("value", this.Value);
			return xElement;
		}
	}
}
