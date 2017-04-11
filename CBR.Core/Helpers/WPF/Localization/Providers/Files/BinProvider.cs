using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.IO;

namespace CBR.Core.Helpers.Localization
{
    /// <summary>
    /// Resource provider based on binary files and the model LocalizationFile
    /// </summary>
    class BinProvider : FileBaseProvider
    {
        #region ----------------CONSTRUCTOR----------------

        /// <summary>
        /// Constructor
        /// </summary>
        public BinProvider()
            : base()
		{
			ResourceFolder = DirectoryHelper.LanguagePath;
            FileExtension = "bin";
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
						BinaryHelper.Serialize(Path.Combine(this.ResourceFolder, dico.FileName), dico);
                    }
                }
            }
            catch (Exception err)
            {
                LogHelper.Manage("BinProvider:SaveDefaultResources", err);
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
				foreach (string file in Directory.GetFiles(ResourceFolder, "*.bin"))
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
                LocalizationDictionary dict = (LocalizationDictionary)BinaryHelper.Deserialize(file);
                dict.FileName = Path.GetFileName(file);

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
