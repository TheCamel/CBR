using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace CBR.Core.Helpers.Localization
{
    public class CultureEventArgs : EventArgs
    {
        public CultureInfo Culture { get; set; }
    }

    /// <summary>
    /// Culture event delegate signature
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void CultureEventArrived(object sender, CultureEventArgs e);

    /// <summary>
    /// Class that manage culture providers (act as a factory) and the UI language. Provide helper functions
    /// to get lists and allow to implement a resource provider
    /// </summary>
    public class CultureManager
    {
        #region ----------------SINGLETON----------------
		/// <summary>
		/// Singleton
		/// </summary>
		public static readonly CultureManager Instance = new CultureManager();

		/// <summary>
		/// Private constructor for singleton pattern
		/// </summary>
        private CultureManager()
		{
		}
		#endregion

        #region ----------------PROPERTIES----------------

        private CultureInfo _uiCulture;

        /// <summary>
        /// Sets the UICulture for the WPF application and raises the <see cref="UICultureChanged"/>
        /// event causing any XAML elements using the <see cref="ResxExtension"/> to automatically
        /// update
        /// </summary>
        public CultureInfo UICulture
        {
            get
            {
                if (_uiCulture == null)
                {
                    _uiCulture = Thread.CurrentThread.CurrentUICulture;
                }
                return _uiCulture;
            }
            set
            {
                if (value != UICulture)
                {
					if (LogHelper.CanDebug())
						LogHelper.Begin("CultureManager.UICulture");
					try
					{
						_uiCulture = value;
						Thread.CurrentThread.CurrentUICulture = value;
						if (SynchronizeThreadCulture)
							SetThreadCulture(value);
						
						//UICultureExtension.UpdateAllTargets();
						MarkupExtensionManager.Instance.UpdateAllTargets();

						if (UICultureChanged != null)
							UICultureChanged(null, new CultureEventArgs()
							{
								Culture = CultureManager.Instance.GetAvailableCultures()
									.First(p => p.IetfLanguageTag == _uiCulture.IetfLanguageTag)
							});
					}
					catch (Exception err)
					{
						LogHelper.Manage("CultureManager.UICulture", err);
					}
					finally
					{
						LogHelper.End("CultureManager.UICulture");
					}  
                }
            }
        }

        private bool _synchronizeThreadCulture = true;

        /// <summary>
        /// If set to true then the <see cref="Thread.CurrentCulture"/> property is changed
        /// to match the current <see cref="UICulture"/>
        /// </summary>
        public bool SynchronizeThreadCulture
        {
            get { return _synchronizeThreadCulture; }
            set
            {
                _synchronizeThreadCulture = value;
                if (value)
                {
                    SetThreadCulture(UICulture);
                }
            }
        }

        /// <summary>
        /// internal resource provider
        /// </summary>
        internal IResourceProvider _resProvider = null;

        /// <summary>
        /// Current resource provider
        /// </summary>
        internal IResourceProvider Provider
        {
            get { if (_resProvider == null) _resProvider = GetResourceProvider(); return _resProvider; }
            set { _resProvider = value; }
        }

        #endregion

        #region ----------------EVENTS----------------

        /// <summary>
        /// Raised when the <see cref="UICulture"/> is changed
        /// </summary>
        /// <remarks>
        /// Since this event is static if the client object does not detach from the event a reference
        /// will be maintained to the client object preventing it from being garbage collected - thus
        /// causing a potential memory leak. 
        /// </remarks>
        public event CultureEventArrived UICultureChanged;

        /// <summary>
        /// Raised if the available culture list change
        /// </summary>
        public event CultureEventArrived AvailableCulturesChanged;

        #endregion

        #region ----------------METHODS----------------

		/// <summary>
		/// Get the nearest code regarding the current culture
		/// </summary>
		/// <param name="ietfCode"></param>
		/// <returns></returns>
		public string GetNearestCode()
		{
			if (LogHelper.CanDebug())
				LogHelper.Begin("CultureManager.GetNearestCode");
			try
			{
				// the 3 languague we support in CBR
				List<string> ietfCodeList = new List<string>() { "en", "fr", "de" };

				foreach (string code in ietfCodeList)
				{
					//current code exist in the list
					if (code == _uiCulture.IetfLanguageTag)
						return code;

					//language is the same, let country left
					if (code == _uiCulture.IetfLanguageTag.Substring(0, 2))
						return code;
				}

				//nothing, return english
				return ietfCodeList.First(p => p == "en");
			}
			catch (Exception err)
			{
				LogHelper.Manage("CultureManager.GetNearestCode", err);
				return string.Empty;
			}
			finally
			{
				LogHelper.End("CultureManager.GetNearestCode");
			}  
		}

        public void Refresh()
        {
			try
			{
				MarkupExtensionManager.Instance.UpdateAllTargets();
			}
			catch (Exception err)
			{
				LogHelper.Manage("CultureManager.Refresh", err);
			}
        }

        /// <summary>
        /// Return a list of CultureItem discovered by the provider
        /// </summary>
        /// <returns></returns>
		public List<CultureInfo> GetAvailableCultures()
        {
			try
			{
				return Provider.GetAvailableCultures();
			}
			catch (Exception err)
			{
				LogHelper.Manage("CultureManager.GetAvailableCultures", err);
				return null;
			}
        }

        /// <summary>
        /// Return a list of module or resx discovered by the provider
        /// </summary>
		/// <param name="ietfCode"></param>
        /// <returns></returns>
		public List<string> GetAvailableModules(string ietfCode)
        {
			try
			{
				return Provider.GetAvailableModules(ietfCode);
			}
			catch (Exception err)
			{
				LogHelper.Manage("CultureManager.GetAvailableModules", err);
				return null;
			}
        }

        /// <summary>
        /// Return the resource list of LocalizationItem for a given module
        /// </summary>
		/// <param name="ietfCode"></param>
        /// <param name="modul"></param>
        /// <returns></returns>
		public List<LocalizationItem> GetModuleResource(string ietfCode, string modul)
        {
			try
			{
				return Provider.GetModuleResource(ietfCode, modul);
			}
			catch (Exception err)
			{
				LogHelper.Manage("CultureManager.GetModuleResource", err);
				return null;
			}
        }

        /// <summary>
        /// Ask the provider to save the resource (if possible...)
        /// </summary>
        public void SaveResources()
		{
			try
			{
				Provider.SaveDefaultResources();
			}
			catch (Exception err)
			{
				LogHelper.Manage("CultureManager.SaveResources", err);
			}
        }

        /// <summary>
        /// This remove a resource from memory, more for developper when update or change the module
        /// or the default value to re create the resource (files mode)
        /// </summary>
        /// <param name="resource"></param>
		public void DeleteResource(string ietfCode, string modul, object resource)
        {
			try
			{
				Provider.DeleteResource(ietfCode, modul, resource);
			}
			catch (Exception err)
			{
				LogHelper.Manage("CultureManager.DeleteResource", err);
			}
        }

		public void DeleteCulture(string ietfCode)
		{
			try
			{
				Provider.DeleteCulture(ietfCode);
			}
			catch (Exception err)
			{
				LogHelper.Manage("CultureManager.DeleteCulture", err);
			}
		}

        /// <summary>
        /// Ask the provider (if possible...) to create a new culture resource, need to be saved later
        /// </summary>
        /// <param name="info"></param>
        public void CreateCulture(CultureInfo info)
        {
			try
			{
				if (_resProvider != null)
				{
					CultureInfo item = _resProvider.CreateCulture(info);
					if (AvailableCulturesChanged != null)
					{
						AvailableCulturesChanged(null, new CultureEventArgs() { Culture = item });
					}
				}
			}
			catch (Exception err)
			{
				LogHelper.Manage("CultureManager.CreateCulture", err);
			}
        }

		public string GetLocalization(string modul, string key, string defaultValue)
		{
			try
			{
				return Provider.GetLocalizationResource(UICulture.IetfLanguageTag, modul, key, defaultValue);
			}
			catch (Exception err)
			{
                LogHelper.Manage("CultureManager.GetLocalization", err);
				return null;
			}
		}

        #endregion

        #region ----------------INTERNALS----------------

        /// <summary>
        /// Retreive a provider based on the configuration
        /// </summary>
        /// <returns></returns>
        private IResourceProvider GetResourceProvider()
        {
			if (LogHelper.CanDebug())
				LogHelper.Begin("CultureManager.GetResourceProvider");
			try
			{
				switch (CBR.Core.Properties.Settings.Default.LocalizeProvider)
				{
					case ProviderMode.RESX:
						return new ResxProvider();

					case ProviderMode.XML:
						return new XmlProvider();

					case ProviderMode.BIN:
						return new BinProvider();

					default: return null;
				}
			}
			catch (Exception err)
			{
				LogHelper.Manage("CultureManager.GetResourceProvider", err);
				return null;
			}
			finally
			{
				LogHelper.End("CultureManager.GetResourceProvider");
			}  
        }

        /// <summary>
        /// Set the thread culture to the given culture
        /// </summary>
        /// <param name="value">The culture to set</param>
        /// <remarks>If the culture is neutral then creates a specific culture</remarks>
        private void SetThreadCulture(CultureInfo value)
        {
            if (value.IsNeutralCulture)
            {
                Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(value.Name);
            }
            else
            {
                Thread.CurrentThread.CurrentCulture = value;
            }
        }
        #endregion
    }
}
