using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using CBR.Core.Helpers;

namespace CBR.Core.Formats.ePUB
{
	public class ePUBMetaDcItem
	{
		private string _name;
		private string _value;

		private System.Collections.Generic.IDictionary<string, string> _attributes;
		private System.Collections.Generic.IDictionary<string, string> _opfAttributes;

		internal ePUBMetaDcItem(string name, string value)
		{
			this._name = name;
			this._value = value;
			this._attributes = new System.Collections.Generic.Dictionary<string, string>();
			this._opfAttributes = new System.Collections.Generic.Dictionary<string, string>();
		}

		internal void SetAttribute(string name, string value)
		{
			this._attributes.Add(name, value);
		}

		internal void SetOpfAttribute(string name, string value)
		{
			this._opfAttributes.Add(name, value);
		}

		internal XElement ToElement()
		{
			XElement xElement = new XElement(ePUBHelper.XmlNamespaces.MetaDC + this._name, this._value);
			foreach (string current in this._opfAttributes.Keys)
			{
				string value = this._opfAttributes[current];
				xElement.SetAttributeValue(ePUBHelper.XmlNamespaces.MetaOPF + current, value);
			}
			foreach (string current2 in this._attributes.Keys)
			{
				string value2 = this._attributes[current2];
				xElement.SetAttributeValue(current2, value2);
			}
			return xElement;
		}
	}
}
