using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System;

namespace CBR.Core.Helpers.Localization
{
    internal abstract class FileBaseProvider : ProviderBase
    {
        #region ----------------PROPERTIES----------------

        /// <summary>
        /// internal storage of resources
        /// </summary>
        protected List<LocalizationFile> _localizationFileList = new List<LocalizationFile>();

		/// <summary>
		/// Resource folder
		/// </summary>
		public string ResourceFolder { get; set; }

        /// <summary>
        /// File extension associated with file storage type
        /// </summary>
        public string FileExtension { get; set; }

        #endregion

        #region ----------------OVERRIDEABLES----------------

		/// <summary>
		/// Extra method to get translation from code
		/// </summary>
		/// <param name="ietfCode"></param>
		/// <param name="modul"></param>
		/// <param name="key"></param>
		/// <param name="defaultValue"></param>
		/// <returns></returns>
		public override string GetLocalizationResource(string ietfCode, string modul, string key, string defaultValue)
		{
			List<LocalizationExtension> ext = MarkupExtensionManager.Instance.Extensions.
				Cast<LocalizationExtension>().Where(p => p.ResModul == modul && p.Key == key).ToList();

			LocalizationExtension locExt;
			if (ext.Count == 1)
				locExt = ext[0];
			else
				locExt = new LocalizationExtension(modul, key, defaultValue);

			LocalizationItem item = GetCorrespondingItem(locExt, CultureInfo.GetCultureInfo(ietfCode));

			return item.Translated;
		}

        /// <summary>
        /// This remove a resource from memory, more for developper when update or change the module
        /// or the default value to re create the resource (files mode)
        /// </summary>
		/// <param name="ietfCode"></param>
        /// <param name="modul"></param>
        /// <param name="resource"></param>
		public override void DeleteResource(string ietfCode, string modul, object resource)
        {
            LocalizationItem item = resource as LocalizationItem;
			LocalizationDictionary dico = GetLocalizationDictionary(ietfCode, modul);

            dico.LocalizationItems.Remove(item);
        }

        /// <summary>
        /// Return all resources for a given module, not implemented by all providers
        /// </summary>
		/// <param name="ietfCode"></param>
        /// <param name="modul"></param>
        /// <returns></returns>
		public override List<LocalizationItem> GetModuleResource(string ietfCode, string modul)
        {
			return GetLocalizationItem(ietfCode, modul);
        }

        /// <summary>
        /// Return all discovered moduls, not implemented by all providers
        /// </summary>
		/// <param name="ietfCode"></param>
        /// <returns></returns>
        public override List<string> GetAvailableModules(string ietfCode)
        {
            List<string> result = new List<string>();

            try
            {
				List<LocalizationFile> loclist = _localizationFileList.Where(p => p.IetfLanguageTag == ietfCode).ToList();
                foreach( LocalizationFile locfile in loclist )
                {
                    result.AddRange( locfile.Dictionnaries.Select( p => p.Module ).Distinct().ToList() );
                }
            }
            catch (Exception err)
            {
                LogHelper.Manage("FileBaseProvider:GetAvailableModules", err);
            }

            return result.Distinct().ToList();
        }

        /// <summary>
        /// Return discovered culture, not implemented by all providers
        /// </summary>
        /// <returns></returns>
		public override List<CultureInfo> GetAvailableCultures()
        {
            List<LocalizationFile> result = _localizationFileList;

            //no language
            if (result.Count <= 0)
                return null;

            return result.Select(p => CultureInfo.GetCultureInfo(p.IetfLanguageTag)).ToList();
        }

        /// <summary>
        /// Create new culture resources, not implemented by all providers
        /// </summary>
        /// <param name="info"></param>
        public override CultureInfo CreateCulture(CultureInfo info)
        {
            //create new file
			LocalizationFile newFile = new LocalizationFile(info.IetfLanguageTag);
            
            //find the english reference
			LocalizationFile maxDict = _localizationFileList.Single(p => p.IetfLanguageTag == "en");

            //copy all dictionnaries
            foreach( LocalizationDictionary dico in maxDict.Dictionnaries )
            {
				LocalizationDictionary clone = dico.Clone(newFile.IetfLanguageTag, FileExtension);
                newFile.Dictionnaries.Add(clone);
            }

            //add file to the list
            _localizationFileList.Add(newFile);

			return CultureInfo.GetCultureInfo(newFile.IetfLanguageTag);
        }

		public override void DeleteCulture(string ietfCode)
		{
			LocalizationFile oldFile = _localizationFileList.Single(p => p.IetfLanguageTag == ietfCode);
			
			//copy all dictionnaries
			foreach (LocalizationDictionary dico in oldFile.Dictionnaries)
			{
				string fullPath = Path.Combine(this.ResourceFolder, dico.FileName);
				if (File.Exists(fullPath))
					File.Delete(fullPath);
			}

			_localizationFileList.Remove(oldFile);
		}

        /// <summary>
        /// Return the extension value, implemented by all providers
        /// </summary>
        /// <param name="ext"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public override object GetObject(LocalizationExtension ext, CultureInfo culture)
        {
			List<LocalizationItem> listLoc = GetLocalizationItem(culture.IetfLanguageTag, ext.ResModul);
            if (listLoc != null)
            {
				LocalizationItem item = listLoc.Single(p => p.Key == ext.Key);
				item.IsUsed = true;
                return item.Translated;
            }
            else
            {
				listLoc = GetLocalizationItem(culture.IetfLanguageTag, "UNDEFINED");
				if (listLoc != null)
				{
					LocalizationItem item = listLoc.Single(p => p.Key == ext.Key);
					item.IsUsed = true;
					return item.Translated;
				}
				else
					return null;
            }
        }

        /// <summary>
        ///  Return the default value for the property
        /// </summary>
        /// <param name="ext"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public override object GetDefaultValue(LocalizationExtension ext, CultureInfo culture)
        {
			return GetCorrespondingItem(ext, culture).Default;
        }

		internal LocalizationItem GetCorrespondingItem(LocalizationExtension ext, CultureInfo culture)
		{
            if (string.IsNullOrEmpty(ext.DefaultValue))
                ext.DefaultValue = (string)base.GetDefaultValue(ext, culture);
			
            //add default value to the resource
			LocalizationItem itemResult = new LocalizationItem()
                { Key = ext.Key, Default = ext.DefaultValue, Translated = ext.DefaultValue };

			List<LocalizationItem> listLoc = null;

			if (string.IsNullOrEmpty(ext.ResModul)) //case modul is missing, invent a undefined.2letter.xml dictionnary
			{
				listLoc = GetLocalizationItem(culture.IetfLanguageTag, "UNDEFINED");

				//no resource dictionnary for code and modul
				if (listLoc == null)
				{
					//create it
					LocalizationDictionary dict = GetUndefined(culture.IetfLanguageTag);
					dict.LocalizationItems.Add(itemResult);
				}
				else //list exist, what about the resource
				{
					if (listLoc.Exists(p => p.Key == ext.Key))
						return listLoc.Single(p => p.Key == ext.Key);
					else
						listLoc.Add(itemResult);
				}

				itemResult.IsUsed = true;
				return itemResult;
			}

			LocalizationFile loc_file = GetLocalizationFileFromCode(culture.IetfLanguageTag);

			//file is missing
			if (loc_file == null)
			{
				//create the file
				loc_file = new LocalizationFile(culture.IetfLanguageTag);
				_localizationFileList.Add(loc_file);

				//create the dico
				LocalizationDictionary dict = new LocalizationDictionary()
				{
					Module = ext.ResModul,
					IetfLanguageTag = culture.IetfLanguageTag,
					FileName = string.Format("{0}.{1}.{2}", ext.ResModul, culture.IetfLanguageTag, FileExtension),
					LocalizationItems = new List<LocalizationItem>()
				};

				loc_file.Dictionnaries.Add(dict);

				dict.LocalizationItems.Add(itemResult);

				itemResult.IsUsed = true;
				return itemResult;
			}

			LocalizationDictionary dico = GetLocalizationDictionary(culture.IetfLanguageTag, ext.ResModul);

			// no dictionnary
			if (dico == null)
			{
				//create the dico
				dico = new LocalizationDictionary()
				{
					Module = ext.ResModul,
					IetfLanguageTag = culture.IetfLanguageTag,
					FileName = string.Format("{0}.{1}.{2}", ext.ResModul, culture.IetfLanguageTag, FileExtension),
					LocalizationItems = new List<LocalizationItem>()
				};

				loc_file.Dictionnaries.Add(dico);

				dico.LocalizationItems.Add(itemResult);

				itemResult.IsUsed = true;
				return itemResult;
			}
			else
			{
				List<LocalizationItem> temp = dico.LocalizationItems.Where(p => p.Key == itemResult.Key).ToList();
				if (temp.Count == 1)
					itemResult = temp[0];
				else
				{
					dico.LocalizationItems.Add(itemResult);
					CreateForOtherlanguage(itemResult, culture.IetfLanguageTag, ext.ResModul);
				}
			}
			
			itemResult.IsUsed = true;
			return itemResult;
		}

		protected void CreateForOtherlanguage(LocalizationItem item, string ietfCodeOther, string modul)
		{
			try
			{
				foreach (LocalizationFile fileOther in _localizationFileList.Where(p => p.IetfLanguageTag != ietfCodeOther))
				{
					List<LocalizationExtension> ext = MarkupExtensionManager.Instance.Extensions.
						Cast<LocalizationExtension>().Where(p => p.ResModul == modul && p.Key == item.Key).ToList();

					LocalizationExtension locExt;
					if (ext.Count == 1)
						locExt = ext[0];
					else
						locExt = new LocalizationExtension(modul, item.Key, item.Default);

					GetCorrespondingItem(locExt, CultureInfo.GetCultureInfo(fileOther.IetfLanguageTag));
				}
			}
			catch { }
		}
        #endregion

        #region ----------------INTERNALS----------------

        /// <summary>
        /// return the UNDEFINED modul dictionnary
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        protected LocalizationDictionary GetUndefined(string code)
        {
            try
            {
                LocalizationDictionary dict = GetLocalizationDictionary(code, "UNDEFINED");
                if (dict == null)
                {
                    LocalizationFile fileLoc = GetLocalizationFileFromCode(code);
                    if (fileLoc != null)
                    {
                        dict = new LocalizationDictionary()
                        {
                            Module = "UNDEFINED",
							IetfLanguageTag = code,
                            FileName = string.Format("{0}.{1}.{2}", "UNDEFINED", code, FileExtension),
                            LocalizationItems = new List<LocalizationItem>()
                        };

                        fileLoc.Dictionnaries.Add(dict);
                    }
                    else return null;
                }
                
                return dict;
            }
            catch { return null; }
        }

        /// <summary>
        /// return a LocalizationFile from cache regarding the given code
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        protected LocalizationFile GetLocalizationFileFromCode(string code)
        {
            try
            {
				return _localizationFileList.Where(p => p.IetfLanguageTag == code).First();
            }
            catch { return null; }
        }

        /// <summary>
        /// return a LocalizationDictionary from cache regarding the given code and module
        /// </summary>
        /// <param name="code"></param>
        /// <param name="modul"></param>
        /// <returns></returns>
        protected LocalizationDictionary GetLocalizationDictionary(string code, string modul)
        {
            try
            {
				return _localizationFileList.Where(p => p.IetfLanguageTag == code).First()
                        .Dictionnaries.Where(d => d.Module == modul).First();
            }
            catch { return null; }
        }

        /// <summary>
        /// return a list of LocalizationItem from cache regarding the given code and module
        /// </summary>
        /// <param name="code"></param>
        /// <param name="modul"></param>
        /// <returns></returns>
        protected List<LocalizationItem> GetLocalizationItem(string code, string modul)
        {
            try
            {
				return _localizationFileList.Where(p => p.IetfLanguageTag == code).First()
                        .Dictionnaries.Where( d => d.Module == modul).First().LocalizationItems;
            }
            catch { return null; }
        }

        #endregion
    }
}
