using System;
using System.Xml.Serialization;

namespace CBR.Core.Models
{
	/// <summary>
	/// frame type
	/// </summary>
	public enum FrameType
	{
		Zone, Page
	}

	/// <summary>
	/// Zone class that represent a part of dynamic book animation
	/// </summary>
    [Serializable, XmlRoot(ElementName = "config")]
	public class Zone
	{
		public Zone()
		{
            Type = FrameType.Zone;
		}

		public Zone(string filePath)
		{
            Type = FrameType.Zone;
            FilePath = filePath;
		}

        [XmlAttribute]
		public string FilePath { get; set; }

        [XmlAttribute]
        public FrameType Type { get; set; }

		/// <summary>
		/// top/left corner
		/// </summary>
        [XmlAttribute]
        public double X { get; set; }
        
        [XmlAttribute]
        public double Y { get; set; }

        [XmlAttribute]
        public double Width { get; set; }

        [XmlAttribute]
        public double Height { get; set; }

        [XmlAttribute]
        public int OrderNum { get; set; }

        [XmlAttribute]
        public int Duration { get; set; }
	}
}
