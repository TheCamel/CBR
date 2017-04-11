using System.Collections.Generic;
using System.Globalization;

namespace CBR.Core.Helpers.Localization
{
    /// <summary>
    /// Available providers
    /// </summary>
    public enum ProviderMode
    {
        RESX, XML, BIN
    }

    /// <summary>
    /// Resource provider interface
    /// </summary>
    interface IResourceProvider
    {
		/// <summary>
		/// ask by code for specific resource key
		/// </summary>
		/// <param name="ietfCode"></param>
		/// <param name="modul"></param>
		/// <param name="key"></param>
		/// <param name="defaultValue"></param>
		/// <returns></returns>
		string GetLocalizationResource(string ietfCode, string modul, string key, string defaultValue);

        /// <summary>
        /// This remove a resource from memory, more for developper when update or change the module
        /// or the default value to re create the resource (files mode)
        /// </summary>
		/// <param name="ietfCode"></param>
        /// <param name="modul"></param>
        /// <param name="resource"></param>
		void DeleteResource(string ietfCode, string modul, object resource);
        
            /// <summary>
        /// Create new culture resources, not implemented by all providers
        /// </summary>
        /// <param name="info"></param>
		CultureInfo CreateCulture(CultureInfo info);

		/// <summary>
		/// delete a culture, dictionnaries and files (sources)
		/// </summary>
		/// <param name="ietfCode"></param>
		void DeleteCulture(string ietfCode);

        /// <summary>
        /// Save the resource, not implemented by all providers
        /// </summary>
        void SaveDefaultResources();

        /// <summary>
        /// Return discovered culture, not implemented by all providers
        /// </summary>
        /// <returns></returns>
		List<CultureInfo> GetAvailableCultures();

        /// <summary>
        /// Return all discovered moduls, not implemented by all providers
        /// </summary>
		/// <param name="ietfCode"></param>
        /// <returns></returns>
		List<string> GetAvailableModules(string ietfCode);

        /// <summary>
        /// Return all resources for a given module, not implemented by all providers
        /// </summary>
		/// <param name="ietfCode"></param>
        /// <param name="modul"></param>
        /// <returns></returns>
		List<LocalizationItem> GetModuleResource(string ietfCode, string modul);

        /// <summary>
        /// Return the extension value, implemented by all providers
        /// </summary>
        /// <param name="ext"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        object GetObject(LocalizationExtension ext, CultureInfo culture);

        /// <summary>
        /// Convert the extession value regarding the destination binding, implemented by all providers
        /// </summary>
        /// <param name="ext"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        object ConvertValue(LocalizationExtension ext, object value);

        /// <summary>
        /// return the default value of a given extension, implemented by all providers, implemented by all providers
        /// </summary>
        /// <param name="ext"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        object GetDefaultValue(LocalizationExtension ext, CultureInfo culture);
    }
}
