using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Diagnostics;

namespace CBR.Core.Helpers
{
    /// <summary>
    /// This class creates a weak delegate of form Action(Of Object)
    /// </summary>
    public class WeakAction
    {
        #region Data
        private readonly WeakReference _target;
        private readonly Type _ownerType;
        private readonly Type _actionType;
        private readonly string _methodName;
        #endregion

        #region Public Properties/Methods
        public WeakAction(object target, Type actionType, MethodBase mi)
        {
            if (target == null)
            {
                Debug.Assert(mi.IsStatic);
                _ownerType = mi.DeclaringType;
            }
            else
                _target = new WeakReference(target);
            _methodName = mi.Name;
            _actionType = actionType;
        }

        public Type ActionType
        {
            get { return _actionType; }
        }

        public bool HasBeenCollected
        {
            get
            {
                return (_ownerType == null && (_target == null || !_target.IsAlive));
            }
        }

        public Delegate GetMethod()
        {
            if (_ownerType != null)
            {
                return Delegate.CreateDelegate(_actionType, _ownerType, _methodName);
            }

            if (_target != null && _target.IsAlive)
            {
                object target = _target.Target;
                if (target != null)
                    return Delegate.CreateDelegate(_actionType, target, _methodName);
            }

            return null;
        }
        #endregion
    }
}
