using System.IO;
using System;
using System.Threading.Tasks;

namespace CBR.Core.Helpers.Localization
{
    /// <summary>
    /// Resource provider based on xml files and the model LocalizationFile
    /// </summary>
    class XmlProvider : FileBaseProvider
    {
        #region ----------------CONSTRUCTOR----------------

        /// <summary>
        /// Constructor
        /// </summary>
        public XmlProvider() : base()
		{
			ResourceFolder = DirectoryHelper.LanguagePath;
            FileExtension = "xml";
            LoadDictionnaries();
		}
        #endregion

        #region ----------------OVERRIDEABLES----------------

        /// <summary>
        /// Save the resource, not implemented by all providers
        /// </summary>
        public override void SaveDefaultResources()
        {
            try
            {
                foreach (LocalizationFile fil in _localizationFileList)
                {
                    foreach (LocalizationDictionary dico in fil.Dictionnaries)
                    {
                        XmlHelper.Serialize(Path.Combine( this.ResourceFolder, dico.FileName), dico);
                    }
                }
            }
            catch (Exception err)
            {
                LogHelper.Manage("XmlProvider:SaveDefaultResources", err);
            }
        }

        #endregion

        #region ----------------INTERNALS----------------

        /// <summary>
        /// Load all xml files in the language folder from settings
        /// </summary>
        protected void LoadDictionnaries()
        {
            try
            {
				foreach( string file in Directory.GetFiles(ResourceFolder, "*.xml") ) 
				{
					LoadDictionnary(file);
				}

            }
            catch (Exception err)
            {
                //no log because of vs designer
            }
        }

        /// <summary>
        /// Load a given file
        /// </summary>
        /// <param name="file"></param>
        private void LoadDictionnary(string file)
        {
            try
            {
                LocalizationDictionary dict = (LocalizationDictionary)XmlHelper.Deserialize(file, typeof(LocalizationDictionary));
                dict.FileName = Path.GetFileName(file);

				//controlling, reject
				if (string.IsNullOrEmpty(dict.IetfLanguageTag))
					return;
				if (string.IsNullOrEmpty(dict.Module))
					return;
				if (string.IsNullOrEmpty(dict.FileName))
					return;

				LocalizationFile loc_file = GetLocalizationFileFromCode(dict.IetfLanguageTag);
                if (loc_file == null)
                {
					loc_file = new LocalizationFile(dict.IetfLanguageTag);
                    _localizationFileList.Add(loc_file);
                }

                loc_file.Dictionnaries.Add(dict);
            }
            catch (Exception err)
            {
                //no log because of vs designer
            }
        }

        #endregion
    }
}
