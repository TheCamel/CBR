using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Markup;

[assembly: XmlnsDefinition("http://schemas.microsoft.com/winfx/2006/xaml/presentation", "CBR.Core.Helpers.Localization")]
[assembly: XmlnsDefinition("http://schemas.microsoft.com/winfx/2007/xaml/presentation", "CBR.Core.Helpers.Localization")]
[assembly: XmlnsDefinition("http://schemas.microsoft.com/winfx/2008/xaml/presentation", "CBR.Core.Helpers.Localization")]

namespace CBR.Core.Helpers.Localization
{
    /// <summary>
    /// Defines a base class for markup extensions which are managed by a central 
    /// <see cref="MarkupExtensionManager"/>.   This allows the associated markup targets to be
    /// updated via the manager.
    /// </summary>
    /// <remarks>
    /// The ManagedMarkupExtension holds a weak reference to the target object to allow it to update 
    /// the target.  A weak reference is used to avoid a circular dependency which would prevent the
    /// target being garbage collected.  
    /// </remarks>
    public abstract class ManagedMarkupExtension : MarkupExtension
    {
        #region Member Variables

        /// <summary>
        /// List of weak reference to the target DependencyObjects to allow them to 
        /// be garbage collected
        /// </summary>
        private List<WeakReference> _targetObjects = new List<WeakReference>();

        /// <summary>
        /// The target property 
        /// </summary>
        private object _targetProperty;

        #endregion

        #region Public Interface

        /// <summary>
        /// Create a new instance of the markup extension
        /// </summary>
        public ManagedMarkupExtension()
        {
            MarkupExtensionManager.Instance.RegisterExtension(this);
        }

        /// <summary>
        /// Return the value for this instance of the Markup Extension
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var targetHelper = serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;
            if (targetHelper.TargetObject != null)
            {
                _targetProperty = targetHelper.TargetProperty;

                if (targetHelper.TargetObject is DependencyObject || !(_targetProperty is DependencyProperty))
                {
                    _targetObjects.Add(new WeakReference(targetHelper.TargetObject));
                    return GetValue();
                }
                else
                {
                    // the extension is being used in a template
                    //
                    return this;
                }
            }
            return null;
        }

        /// <summary>
        /// Update the associated target
        /// </summary>
        public void UpdateTarget()
        {
            if (_targetProperty != null)
            {
                foreach (WeakReference reference in _targetObjects)
                {
                    if (_targetProperty is DependencyProperty)
                    {
                        DependencyObject target = reference.Target as DependencyObject;
                        if (target != null)
                        {
                            target.SetValue(_targetProperty as DependencyProperty, GetValue());
                        }
                    }
                    else if (_targetProperty is PropertyInfo)
                    {
                        object target = reference.Target;
                        if (target != null)
                        {
                            (_targetProperty as PropertyInfo).SetValue(target, GetValue(), null);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Is an associated target still alive ie not garbage collected
        /// </summary>
        public bool IsTargetAlive
        {
            get 
            {
                // for normal elements the _targetObjects.Count will always be 1
                // for templates the Count may be zero if this method is called
                // in the middle of window elaboration after the template has been
                // instantiated but before the elements that use it have been.  In
                // this case return true so that we don't unhook the extension
                // prematurely
                //
                if (_targetObjects.Count == 0)
                    return true;
                
                // otherwise just check whether the referenced target(s) are alive
                //
                foreach (WeakReference reference in _targetObjects)
                {
                    if (reference.IsAlive) return true;
                }
                return false; 
            } 
        }

        /// <summary>
        /// Returns true if a target attached to this extension is in design mode
        /// </summary>
        public bool IsInDesignMode
        {
            get
            {
                foreach (WeakReference reference in _targetObjects)
                {
                    DependencyObject element = reference.Target as DependencyObject;
                    if (element != null && DesignerProperties.GetIsInDesignMode(element)) return true;
                }
                return false;
            }
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Return the Target Property the extension is associated with
        /// </summary>
        /// <remarks>
        /// Can either be a <see cref="DependencyProperty"/> or <see cref="PropertyInfo"/>
        /// </remarks>
        protected object TargetProperty
        {
            get { return _targetProperty as DependencyProperty; }
        }

        /// <summary>
        /// Return the type of the Target Property
        /// </summary>
        public Type TargetPropertyType
        {
            get
            {
                Type propertyType = null;
                if (_targetProperty is DependencyProperty)
                    propertyType = (_targetProperty as DependencyProperty).PropertyType;
                else if (_targetProperty is PropertyInfo)
                    propertyType =(_targetProperty as PropertyInfo).PropertyType;
                return propertyType;
            }
        }
 
        /// <summary>
        /// Return the value associated with the key from the resource manager
        /// </summary>
        /// <returns>The value from the resources if possible otherwise the default value</returns>
        protected abstract object GetValue();

        #endregion
    }
}
