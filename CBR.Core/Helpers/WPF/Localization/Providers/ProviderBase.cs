using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace CBR.Core.Helpers.Localization
{
    /// <summary>
    /// Base class for all resource providers
    /// </summary>
    abstract internal class ProviderBase : IResourceProvider
    {
        #region ----------------OVERRIDEABLES----------------

		/// <summary>
		/// ask by code for specific resource key
		/// </summary>
		/// <param name="ietfCode"></param>
		/// <param name="modul"></param>
		/// <param name="key"></param>
		/// <param name="defaultValue"></param>
		/// <returns></returns>
		public virtual string GetLocalizationResource(string ietfCode, string modul, string key, string defaultValue)
		{
			throw new NotImplementedException();
		}

		/// <summary>
        /// This remove a resource from memory, more for developper when update or change the module
        /// or the default value to re create the resource (files mode)
        /// </summary>
		/// <param name="ietfCode"></param>
        /// <param name="modul"></param>
        /// <param name="resource"></param>
		public virtual void DeleteResource(string ietfCode, string modul, object resource)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Return all resources for a given module, not implemented by all providers
        /// </summary>
		/// <param name="ietfCode"></param>
        /// <param name="modul"></param>
        /// <returns></returns>
		public virtual List<LocalizationItem> GetModuleResource(string ietfCode, string modul)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Return all discovered moduls, not implemented by all providers
        /// </summary>
		/// <param name="ietfCode"></param>
        /// <returns></returns>
		public virtual List<string> GetAvailableModules(string ietfCode)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Return discovered culture, not implemented by all providers
        /// </summary>
        /// <returns></returns>
        public virtual List<CultureInfo> GetAvailableCultures()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Save the resource, not implemented by all providers
        /// </summary>
        public virtual void SaveDefaultResources()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Create new culture resources, not implemented by all providers
        /// </summary>
        /// <param name="info"></param>
        public virtual CultureInfo CreateCulture(CultureInfo info)
        {
            throw new NotImplementedException();
        }

		/// <summary>
		/// delete a culture, dictionnaries and files (sources)
		/// </summary>
		/// <param name="ietfCode"></param>
		public virtual void DeleteCulture(string ietfCode)
		{
			throw new NotImplementedException();
		}

        /// <summary>
        /// Return the extension value, implemented by all providers
        /// </summary>
        /// <param name="ext"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public virtual object GetObject(LocalizationExtension ext, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region ----------------INTERNALS----------------

        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);

        /// <summary>
        /// Convert a resource object to the type required by the WPF element
        /// </summary>
        /// <param name="value">The resource value to convert</param>
        /// <returns>The WPF element value</returns>
        public object ConvertValue(LocalizationExtension ext, object value)
        {
            object result = null;

			if (LogHelper.CanDebug())
				LogHelper.Begin("ProviderBase.ConvertValue");
			try
			{
				//BitmapSource bitmapSource = null;
				//// convert icons and bitmaps to BitmapSource objects that WPF uses
				//if (value is Icon)
				//{
				//    Icon icon = value as Icon;

				//    // For icons we must create a new BitmapFrame from the icon data stream
				//    // The approach we use for bitmaps (below) doesn't work when setting the
				//    // Icon property of a window (although it will work for other Icons)
				//    //
				//    using (MemoryStream iconStream = new MemoryStream())
				//    {
				//        icon.Save(iconStream);
				//        iconStream.Seek(0, SeekOrigin.Begin);
				//        bitmapSource = BitmapFrame.Create(iconStream);
				//    }
				//}
				//else if (value is Bitmap)
				//{
				//    Bitmap bitmap = value as Bitmap;
				//    IntPtr bitmapHandle = bitmap.GetHbitmap();
				//    bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(bitmapHandle, IntPtr.Zero, Int32Rect.Empty,
				//        BitmapSizeOptions.FromEmptyOptions());
				//    bitmapSource.Freeze();
				//    DeleteObject(bitmapHandle);
				//}

				//if (bitmapSource != null)
				//{
				//    // if the target property is expecting the Icon to be content then we
				//    // create an ImageControl and set its Source property to image
				//    //
				//    if (ext.TargetPropertyType == typeof(object))
				//    {
				//        System.Windows.Controls.Image imageControl = new System.Windows.Controls.Image();
				//        imageControl.Source = bitmapSource;
				//        imageControl.Width = bitmapSource.Width;
				//        imageControl.Height = bitmapSource.Height;
				//        result = imageControl;
				//    }
				//    else
				//    {
				//        result = bitmapSource;
				//    }
				//}
				//else
				{
					result = value;

					// allow for resources to either contain simple strings or typed data
					//
					Type targetType = ext.TargetPropertyType;
					if (value is String && targetType != typeof(String) && targetType != typeof(object))
					{
						TypeConverter tc = TypeDescriptor.GetConverter(targetType);
						result = tc.ConvertFromInvariantString(value as string);
					}
				}

				return result;
			}
			catch (Exception err)
			{
				LogHelper.Manage("ProviderBase.ConvertValue", err);
				return null;
			}
			finally
			{
				LogHelper.End("ProviderBase.ConvertValue");
			}  
        }

        /// <summary>
        ///  Return the default value for the property
        /// </summary>
        /// <param name="ext"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public virtual object GetDefaultValue(LocalizationExtension ext, CultureInfo culture)
        {
			if (LogHelper.CanDebug())
				LogHelper.Begin("ProviderBase.GetDefaultValue", "extension:{0}, culture:{1}", ext.ToString(), culture.DisplayName);
			try
			{
				object result = ext.DefaultValue;
				Type targetType = ext.TargetPropertyType;

				if (ext.DefaultValue == null)
				{
					if (targetType == typeof(String) || targetType == typeof(object))
					{
						result = "No default on #" + ext.Key;
					}
				}
				else if (targetType != null)
				{
					// convert the default value if necessary to the required type
					if (targetType != typeof(String) && targetType != typeof(object))
					{
						try
						{
							TypeConverter tc = TypeDescriptor.GetConverter(targetType);
							result = tc.ConvertFromInvariantString(ext.DefaultValue);
						}
						catch
						{
						}
					}
				}

				return result;
			}
			catch (Exception err)
			{
				LogHelper.Manage("ProviderBase.GetDefaultValue", err);
				return null;
			}
			finally
			{
				LogHelper.End("ProviderBase.GetDefaultValue");
			}  
        }
        #endregion
    }
}
