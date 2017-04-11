using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CBR.Core.Helpers.NET.Properties
{
	public class PropertyModel
	{
		public string Name { get; set; }
		
		public string LabelKey { get; set; }
	
		//public string Prefix { get; set; }

		public bool IsGroup { get; set; }

		public bool IsSort { get; set; }

		public bool IsDynamic { get; set; }
		
		public string FullName
		{
			get
			{
				return IsDynamic ? "Dynamics." + this.Name : this.Name;
			}
		}
	}
}
