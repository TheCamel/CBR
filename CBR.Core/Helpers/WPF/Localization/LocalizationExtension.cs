using System;
using System.Globalization;
using System.Windows.Markup;

namespace CBR.Core.Helpers.Localization
{
    /// <summary>
    /// Defines the handling method for the <see cref="ResxExtension.GetResource"/> event
    /// </summary>
    /// <param name="resxName">The name of the resx file</param>
    /// <param name="key">The resource key within the file</param>
    /// <param name="culture">The culture to get the resource for</param>
    /// <returns>The resource</returns>
    public delegate object GetResourceHandler(string resxName, string key, CultureInfo culture);

    /// <summary>
    /// A markup extension to allow resources for WPF Windows and controls to be retrieved
    /// from a resource file associated with the window or control
    /// </summary>
    [MarkupExtensionReturnType(typeof(object))]
    public class LocalizationExtension : ManagedMarkupExtension
    {
        #region ----------------PROPERTIES----------------

        /// <summary>
        /// The type name that the resource is associated with
        /// </summary>
        private string _resModul;

        /// <summary>
        /// The fully qualified name of the embedded resx (without .resources) to get the resource from
        /// </summary>
        public string ResModul
        {
            get { return _resModul; }
            set { _resModul = value; }
        }

        /// <summary>
        /// The key used to retrieve the resource
        /// </summary>
        private string _key;

        /// <summary>
        /// The name of the resource key
        /// </summary>
        public string Key
        {
            get { return _key; }
            set { _key = value; }
        }

        /// <summary>
        /// The default value for the property
        /// </summary>
        private string _defaultValue;

        /// <summary>
        /// The default value to use if the resource can't be found
        /// </summary>
        /// <remarks>
        /// This particularly useful for properties which require non-null
        /// values because it allows the page to be displayed even if the resource can't be loaded
        /// </remarks>
        public string DefaultValue
        {
			get { CheckDefault(_defaultValue); return _defaultValue; }
            set { _defaultValue = value; }
        }

        #endregion

        #region ----------------EVENTS----------------

        /// <summary>
        /// This event allows a designer or preview application (such as Globalizer.NET) to
        /// intercept calls to get resources and provide the values instead dynamically
        /// </summary>
        public static event GetResourceHandler GetResource;

        #endregion

        #region ----------------CONSTRUCTORS----------------

        /// <summary>
        /// Create a new instance of the markup extension
        /// </summary>
        public LocalizationExtension() : base()
        {
        }

        /// <summary>
        /// Create a new instance of the markup extension
        /// </summary>
        /// <param name="resxName">The fully qualified name of the embedded resx (without .resources)</param>
        /// <param name="key">The key used to get the value from the resources</param>
        /// <param name="defaultValue">
        /// The default value for the property (can be null).  This is useful for non-string
        /// that properties that may otherwise cause page load errors if the resource is not
        /// present for some reason (eg at design time before the assembly has been compiled)
        /// </param>
		public LocalizationExtension(string key, string defaultValue)
			: base()
		{
			if (LogHelper.CanDebug())
				LogHelper.Begin("LocalizationExtension.LocalizationExtension", " key:{0}, defaultValue{2}", key, defaultValue);
			try
			{
				this._key = key;
				CheckDefault(defaultValue);
			}
			catch (Exception err)
			{
				LogHelper.Manage("LocalizationExtension.LocalizationExtension", err);
			}
			finally
			{
				LogHelper.End("LocalizationExtension.LocalizationExtension");
			}  
		}

		/// <summary>
		/// Create a new instance of the markup extension
		/// </summary>
		/// <param name="modul"></param>
		/// <param name="key"></param>
		/// <param name="defaultValue"></param>
		public LocalizationExtension(string modul, string key, string defaultValue)
			: base()
		{
			if (LogHelper.CanDebug())
				LogHelper.Begin("LocalizationExtension.LocalizationExtension", "modul:{0}, key:{1}, defaultValue{2}", modul, key, defaultValue );
			try
			{
				this._resModul = modul;
				this._key = key;
				CheckDefault(defaultValue);
			}
			catch (Exception err)
			{
				LogHelper.Manage("LocalizationExtension.LocalizationExtension", err);
			}
			finally
			{
				LogHelper.End("LocalizationExtension.LocalizationExtension");
			}  
		}

		private void CheckDefault(string defaultValue)
		{
			if (!string.IsNullOrEmpty(defaultValue))
			{
				if (!defaultValue.StartsWith("#"))
					this._defaultValue = "#" + defaultValue;
				else
					this._defaultValue = defaultValue;
			}
		}

        #endregion

        #region ----------------MARKUP EXTENSION OVERRIDE----------------

        /// <summary>
        /// Return the value associated with the key from the resource manager
        /// </summary>
        /// <returns>The value from the resources if possible otherwise the default value</returns>
        protected override object GetValue()
        {
			if (LogHelper.CanDebug())
				LogHelper.Begin("LocalizationExtension.GetValue", "ResModul:{0}, Key:{1}, DefaultValue:{2}", ResModul, Key, DefaultValue);
			
            if (string.IsNullOrEmpty(Key))
                throw new ArgumentException("Key cannot be null");

            object result = null;
            IResourceProvider provider = null;

			if (IsInDesignMode) return DefaultValue;
			try
			{
				object resource = null;

				//allow resource trapping by calling the handler
				if (GetResource != null)
					resource = GetResource(ResModul, Key, CultureManager.Instance.UICulture);

				if (resource == null)
				{
					//get the provider
					if (provider == null)
						provider = CultureManager.Instance.Provider;

					//get the localized resource
					if (provider != null)
						resource = provider.GetObject(this, CultureManager.Instance.UICulture);
				}

				//and then convert it to desired type
				result = provider.ConvertValue(this, resource);
			}
			catch (Exception err)
			{
				LogHelper.Manage("LocalizationExtension.GetValue", err);
			}

            try
            {
                // if it does not work, we ask the default value
                if (result == null)
                    result = provider.GetDefaultValue(this, CultureManager.Instance.UICulture);
            }
            catch (Exception err)
            {
				LogHelper.Manage("LocalizationExtension.GetValue", err);
            }

			LogHelper.End("LocalizationExtension.GetValue");

            return result;
        }
		
		/// <summary>
		/// debug helper
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return string.Format("ResModul:{0}, Key:{1}, DefaultValue:{2}", 
				this.ResModul, this.Key, this.DefaultValue);
		}
        #endregion
    }
}
