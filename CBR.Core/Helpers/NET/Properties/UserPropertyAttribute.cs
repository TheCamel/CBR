using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace CBR.Core.Helpers.NET.Properties
{
	[AttributeUsage(AttributeTargets.Property)]
	public class UserPropertyAttribute : Attribute
	{
		public UserPropertyAttribute(bool canGroup, bool canSort)
		{
			CanGroup = canGroup;
			CanSort = canSort;
		}

		public UserPropertyAttribute(bool canGroup, bool canSort, string labelKey)
		{
			LabelKey = labelKey;
			CanGroup = canGroup;
			CanSort = canSort;
		}

		public string LabelKey { get; set; }
		
		public bool CanView { get; set; }
		
		public bool CanGroup { get; set; }

		public bool CanSort { get; set; }
	}
}
