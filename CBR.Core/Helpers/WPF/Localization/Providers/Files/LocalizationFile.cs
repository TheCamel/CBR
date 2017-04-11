using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;

namespace CBR.Core.Helpers.Localization
{
    /// <summary>
    /// Internal dictionnary grouped by culture code
    /// </summary>
    internal class LocalizationFile
    {
        /// <summary>
        /// constructor for serialize
        /// </summary>
        public LocalizationFile()
        {
        }

        /// <summary>
        /// constructor with init
        /// </summary>
		/// <param name="ietfCode"></param>
		public LocalizationFile(string ietfCode)
        {
			IetfLanguageTag = ietfCode;
            Dictionnaries = new List<LocalizationDictionary>(); 
        }

        /// <summary>
        /// Culture code
        /// </summary>
		public string IetfLanguageTag { get; set; }
        
        /// <summary>
        /// All resources dictionnary associated to a culture
        /// </summary>
        public List<LocalizationDictionary> Dictionnaries { get; set; }
    }

    /// <summary>
    /// LocalizationDictionary identfy a resource file in the file based provider model
    /// that can be serialized to read or produce a file
    /// </summary>
    [Serializable]
    public class LocalizationDictionary
    {
        /// <summary>
        /// filename only, complete resource file path is managed by provider
        /// </summary>
		[XmlIgnore]
        public string FileName { get; set; }

        /// <summary>
        /// culture code, from xml
        /// </summary>
        [XmlAttribute]
		public string IetfLanguageTag { get; set; }

        /// <summary>
        /// Module name, from xml
        /// </summary>
        [XmlAttribute]
        public string Module { get; set; }

        /// <summary>
        /// list of LocalizationItem that contains all module resource
        /// </summary>
        [XmlArray("LocalizationItems")]
        [XmlArrayItem("Item")]
        public List<LocalizationItem> LocalizationItems { get; set; }

        /// <summary>
        /// Clone allow to duplicate a existing resource when create a new culture
        /// </summary>
		/// <param name="newIetfCode"></param>
        /// <returns></returns>
        public LocalizationDictionary Clone(string newIetfCode, string fileExt)
        {
            LocalizationDictionary result = new LocalizationDictionary()
            {
                Module = this.Module,
				IetfLanguageTag = newIetfCode,
				FileName = string.Format("{0}.{1}.{2}", Module, newIetfCode, fileExt)
            };

            result.LocalizationItems = new List<LocalizationItem>();
            foreach (LocalizationItem src in this.LocalizationItems)
                result.LocalizationItems.Add(src.Clone());

            return result;
        }
    }

    /// <summary>
    /// Class that represent a unique resource item in the file based provider model
    /// </summary>
    [Serializable]
    public class LocalizationItem
    {
        /// <summary>
        /// resource key identifier
        /// </summary>
        [XmlAttribute]
        public string Key { get; set; }

        /// <summary>
        /// default value from xaml developper code
        /// </summary>
        [XmlAttribute]
        public string Default { get; set; }

        /// <summary>
        /// Localized resource
        /// </summary>
        [XmlAttribute]
        public string Translated { get; set; }

		private bool _isUsed = false;
		[XmlIgnore]
		public bool IsUsed
		{
			get { return _isUsed; }
			set { _isUsed = value; }
		}

        /// <summary>
        /// Clone allow to duplicate a existing resource when create a new culture
        /// </summary>
        /// <returns></returns>
        public LocalizationItem Clone()
        {
            LocalizationItem loc = new LocalizationItem()
            {
                Key = new string(this.Key.ToArray()),
                Default = new string(this.Default.ToArray()),
            };

			if (!loc.Default.StartsWith("#"))
				loc.Translated = "#" + loc.Default;
			else
				loc.Translated = loc.Default;

			return loc;
        }
    }
}
