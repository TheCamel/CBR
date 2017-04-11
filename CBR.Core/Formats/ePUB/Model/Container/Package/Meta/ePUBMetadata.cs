using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.ComponentModel;

namespace CBR.Core.Formats.ePUB
{
	/// <summary>
	/// Models the Open Packaging Format metadata contained in the package of an epub publication
	/// </summary>
	public class ePUBMetadata
	{
		#region -----------------CONSTRUCTOR-----------------

		/// <summary>
		/// Constructor
		/// </summary>
		public ePUBMetadata()
		{
			Meta = new List<ePUBMetaItem>();
			MetaDC = new List<ePUBMetaDcItem>();
		}

		#endregion

		#region -----------------PROPERTIES-----------------

		[Browsable(true)]
		[Description("Meta items")]
		public List<ePUBMetaItem> Meta{ get; set; }

		[Browsable(true)]
		[Description("Meta DC items ")]
		public List<ePUBMetaDcItem> MetaDC { get; set; }

		#endregion

		#region -----------------METHODS-----------------

//        public void AddMultiItem(
//    ePUBMetadataType type,
//    string value
//){}
//public string GetItem(
//    ePUBMetadataType type
//){}
//public string GetMultiItem(
//    ePUBMetadataType type,
//    int index
//){}
//public string GetUniqueIdentifier(
//    string uniqueIdentifierId
//){}

//public void SetItem(
//    ePUBMetadataType type,
//    string value
//){}
//public void SetMultiItem(
//    ePUBMetadataType type,
//    int index,
//    string value
//){}
//public bool SetUniqueIdentifier(
//    string uniqueIdentifierId,
//    string uniqueIdentifier
//)
//{}
		#endregion
	}
}
