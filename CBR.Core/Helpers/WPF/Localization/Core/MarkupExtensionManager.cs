using System.Collections.Generic;

namespace CBR.Core.Helpers.Localization
{
    /// <summary>
    /// Defines a class for managing <see cref="ManagedMarkupExtension"/> objects
    /// </summary>
    /// <remarks>
    /// This class provides a single point for updating all markup targets that use the given Markup 
    /// Extension managed by this class.   
    /// </remarks>
    public class MarkupExtensionManager 
    {
        #region ----------------SINGLETON----------------

		public static readonly MarkupExtensionManager Instance = new MarkupExtensionManager();

		/// <summary>
		/// Private constructor for singleton pattern
		/// </summary>
        private MarkupExtensionManager()
		{
		}

		#endregion

        #region ----------------PROPERTIES----------------

        /// <summary>
        /// List of active extensions
        /// </summary>
        private List<ManagedMarkupExtension> _extensions = new List<ManagedMarkupExtension>();

        /// <summary>
        /// Return a list of the currently active extensions
        /// </summary>
        public List<ManagedMarkupExtension> Extensions
        {
            get { return _extensions; }
        }

        #endregion

        #region ----------------INTERNALS----------------

        /// <summary>
        /// The number of extensions added since the last cleanup
        /// </summary>
        private int _cleanupCount;

        /// <summary>
        /// The interval at which to cleanup and remove extensions
        /// </summary>
        private int _cleanupInterval = 40;

        #endregion

        #region ----------------PUBLIC METHODS----------------

        /// <summary>
        /// Force the update of all active targets that use the markup extension
        /// </summary>
        public virtual void UpdateAllTargets()
        {
            foreach (ManagedMarkupExtension extension in _extensions)
            {
                extension.UpdateTarget();
            }
        }

        /// <summary>
        /// Cleanup references to extensions for targets which have been garbage collected.
        /// </summary>
        /// <remarks>
        /// This method is called periodically as new <see cref="ManagedMarkupExtension"/> objects 
        /// are registered to release <see cref="ManagedMarkupExtension"/> objects which are no longer 
        /// required (because their target has been garbage collected).  This method does
        /// not need to be called externally, however it can be useful to call it prior to calling
        /// GC.Collect to verify that objects are being garbage collected correctly.
        /// </remarks>
        public void CleanupInactiveExtensions()
        {
            List<ManagedMarkupExtension> newExtensions = new List<ManagedMarkupExtension>(_extensions.Count);
            foreach (ManagedMarkupExtension ext in _extensions)
            {
                if (ext.IsTargetAlive)
                {
                    newExtensions.Add(ext);
                }
            }
            _extensions = newExtensions;
        }

        #endregion

        #region ----------------INTERNALS----------------

        /// <summary>
        /// Register a new extension and remove extensions which reference target objects
        /// that have been garbage collected
        /// </summary>
        /// <param name="extension">The extension to register</param>
        internal void RegisterExtension(ManagedMarkupExtension extension)
        {
            // Cleanup extensions for target objects which have been garbage collected
            // for performance only do this periodically
            //
            if (_cleanupCount > _cleanupInterval)
            {
                CleanupInactiveExtensions();
                _cleanupCount = 0;
            }
            _extensions.Add(extension);
            _cleanupCount++;
        }

        #endregion
    }
}
