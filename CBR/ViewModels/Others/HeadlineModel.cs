using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace CBR.ViewModels
{
    [Serializable]
    public class Headline
    {
        [XmlAttribute]
        public string LinkUri { get; set; }

        [XmlAttribute]
        public string ImageUri { get; set; }

        [XmlAttribute]
        public string Title { get; set; }

        [XmlAttribute]
        public string Description { get; set; }
    }

    [Serializable]
    public class HeadlineCollection
    {
        [XmlArray("HeadlineItems")]
        [XmlArrayItem("Item")]
        public List<Headline> HeadlineItems { get; set; }
    }
}
