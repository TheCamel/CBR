using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.IO;
using System.Windows.Interop;
using System.Globalization;

namespace CBR.Core.Helpers.Localization
{
    class ResxProvider : ProviderBase
    {
        /// <summary>
        /// Cached resource managers
        /// </summary>
        private static Dictionary<string, WeakReference> _resourceManagers = new Dictionary<string, WeakReference>();

        public override object GetObject(LocalizationExtension ext, CultureInfo culture)
        {
            ResourceManager resourceManager = GetResourceManager(ext.ResModul);
            if (resourceManager != null)
            {
                return resourceManager.GetObject(ext.Key, CultureManager.Instance.UICulture);
            }
            else return null;
        }

       
        /// <summary>
        /// Check if the assembly contains an embedded resx of the given name
        /// </summary>
        /// <param name="assembly">The assembly to check</param>
        /// <param name="resxName">The name of the resource we are looking for</param>
        /// <returns>True if the assembly contains the resource</returns>
        private bool HasEmbeddedResx(Assembly assembly, string resxName)
        {
            try
            {
                string[] resources = assembly.GetManifestResourceNames();
                string searchName = resxName.ToLower() + ".resources";
                foreach (string resource in resources)
                {
                    if (resource.ToLower() == searchName) return true;
                }
            }
            catch
            {
                // GetManifestResourceNames throws an exception for some
                // dynamic assemblies - just ignore these assemblies.
            }
            return false;
        }

        /// <summary>
        /// Find the assembly that contains the type
        /// </summary>
        /// <returns>The assembly if loaded (otherwise null)</returns>
        private Assembly FindResourceAssembly(string resxName)
        {
            Assembly assembly = Assembly.GetEntryAssembly();

            // check the entry assembly first - this will short circuit a lot of searching
            //
            if (assembly != null && HasEmbeddedResx(assembly, resxName)) return assembly;

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly searchAssembly in assemblies)
            {
                // skip system assemblies
                //
                string name = searchAssembly.FullName;
                if (!name.StartsWith("Microsoft.") &&
                    !name.StartsWith("System.") &&
                    !name.StartsWith("System,") &&
                    !name.StartsWith("mscorlib,") &&
                    !name.StartsWith("PresentationFramework,") &&
                    !name.StartsWith("WindowsBase,"))
                {
                    if (HasEmbeddedResx(searchAssembly, resxName)) return searchAssembly;
                }
            }
            return null;
        }

        /// <summary>
        /// Get the resource manager for this type
        /// </summary>
        /// <param name="resxName">The name of the embedded resx</param>
        /// <returns>The resource manager</returns>
        /// <remarks>Caches resource managers to improve performance</remarks>
        private ResourceManager GetResourceManager(string resxName)
        {
            WeakReference reference = null;
            ResourceManager result = null;
            if (_resourceManagers.TryGetValue(resxName, out reference))
            {
                result = reference.Target as ResourceManager;

                // if the resource manager has been garbage collected then remove the cache
                // entry (it will be readded)
                //
                if (result == null)
                {
                    _resourceManagers.Remove(resxName);
                }
            }

            if (result == null)
            {
                Assembly assembly = FindResourceAssembly(resxName);
                if (assembly != null)
                {
                    result = new ResourceManager(resxName, assembly);
                }
                _resourceManagers.Add(resxName, new WeakReference(result));
            }
            return result;
        }
    }
}
