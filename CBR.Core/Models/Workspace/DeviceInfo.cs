using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CBR.Core.Files;
using System.Xml.Serialization;

namespace CBR.Core.Models
{
    [Serializable]
    public class DeviceInfo
    {
        #region ----------------DEFAULTs----------------

        public DeviceInfo()
        {
            SupportedFormats = new List<DocumentType>();
        }

        public DeviceInfo( string name, string manu )
        {
            Model = name;
            Manufacturer = manu;
            SupportedFormats = new List<DocumentType>();
        }
        #endregion

        [XmlAttribute]
        public string Manufacturer { get; set; }

        [XmlAttribute]
        public string Model { get; set; }

        public List<DocumentType> SupportedFormats { get; set; }

        [XmlIgnore]
        public bool CanImages
        {
            get { return SupportedFormats.Contains(DocumentType.ImageFile); }
			set { if (value) SupportedFormats.Add(DocumentType.ImageFile); else SupportedFormats.Remove(DocumentType.ImageFile); }
        }
        [XmlIgnore]
        public bool CanRAR
        {
            get { return SupportedFormats.Contains(DocumentType.RARBased); }
			set { if (value) SupportedFormats.Add(DocumentType.RARBased); else SupportedFormats.Remove(DocumentType.RARBased); }
		}
        [XmlIgnore]
        public bool CanZIP
        {
            get { return SupportedFormats.Contains(DocumentType.ZIPBased); }
			set { if (value) SupportedFormats.Add(DocumentType.ZIPBased); else SupportedFormats.Remove(DocumentType.ZIPBased); }
		}
        [XmlIgnore]
        public bool CanPDF
        {
            get { return SupportedFormats.Contains(DocumentType.PDF); }
			set { if (value) SupportedFormats.Add(DocumentType.PDF); else SupportedFormats.Remove(DocumentType.PDF); }
		}

        [XmlIgnore]
        public bool CanXPS
        {
            get { return SupportedFormats.Contains(DocumentType.XPS); }
			set { if (value) SupportedFormats.Add(DocumentType.XPS); else SupportedFormats.Remove(DocumentType.XPS); }
		}
        [XmlIgnore]
        public bool CanEPUB
        {
            get { return SupportedFormats.Contains(DocumentType.ePUB); }
			set { if (value) SupportedFormats.Add(DocumentType.ePUB); else SupportedFormats.Remove(DocumentType.ePUB); }
		}
    }
}
