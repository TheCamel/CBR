using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Xml.Linq;
using CBR.Core.Helpers;
using System.IO;

namespace CBR.Core.Formats.ePUB
{
	/// <summary>
	/// Models the container of an epub publication
	/// </summary>
	public class ePUBContainer
	{
		#region -----------------CONSTRUCTOR-----------------

		/// <summary>
		/// Create an empty container
		/// </summary>
		public ePUBContainer()
		{
		}

		#endregion

		#region -----------------PROPERTIES-----------------

		[Browsable(true)]
		[Description("Package")]
		public ePUBPackage Package { get; set; }

		#endregion

		internal XElement ToElement()
		{
			XNamespace ns = "urn:oasis:names:tc:opendocument:xmlns:container";
			XElement xElement = new XElement(ns + "container", new XAttribute("version", "1.0"));
			XElement xElement2 = new XElement(ns + "rootfiles");
			XElement content = new XElement(ns + "rootfile", new object[]
			{
				new XAttribute("full-path", Package.RelativPackageFolder+'/'+Package.PackageFileName),
				new XAttribute("media-type", ePUBHelper.XmlMediaTypes.OEBPSPackage)
			});
			xElement2.Add(content);
			xElement.Add(xElement2);
			return xElement;
		}
	}
}
