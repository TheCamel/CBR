using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Collections;
using System.Diagnostics;

namespace CBR.Core.Helpers
{
    /// <summary>
    /// Provides change notification, similar to ObservableCollection but without the need to depend on WPF.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class NotificationCollection<T> : Collection<T>, INotifyPropertyChanged
    {
        private SimpleMonitor monitor;

        public NotificationCollection()
        {
            monitor = new SimpleMonitor();
        }

        public NotificationCollection(IEnumerable<T> collection)
        {
            this.monitor = new SimpleMonitor();
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }
            this.CopyFrom(collection);
        }

        public NotificationCollection(List<T> list)
            : base((list != null) ? new List<T>(list.Count) : list)
        {
            this.monitor = new SimpleMonitor();
            this.CopyFrom(list);
        }

        #region Public Members

        [field:NonSerialized]
        public event NotificationCollectionChangedEventHandler CollectionChanged;

        public void Move(int oldIndex, int newIndex)
        {
            this.MoveItem(oldIndex, newIndex);
        }

        #endregion

        #region Protected Members

        [NonSerialized]
        protected PropertyChangedEventHandler PropertyChanged;

        protected IDisposable BlockReentrancy()
        {
            this.monitor.Enter();
            return this.monitor;
        }

        protected void CheckReentrancy()
        {
            if ((this.monitor.Busy && (this.CollectionChanged != null)) && (this.CollectionChanged.GetInvocationList().Length > 1))
            {
                throw new InvalidOperationException("NotificationCollection Reentrancy Not Allowed");
            }
        }

        protected virtual void OnCollectionChanged(NotificationCollectionChangedEventArgs e)
        {
            if (this.CollectionChanged != null)
            {
                using (this.BlockReentrancy())
                {
                    this.CollectionChanged(this, e);
                }
            }
        }

        protected override void ClearItems()
        {
            this.CheckReentrancy();
            base.ClearItems();
            this.OnPropertyChanged("Count");
            this.OnPropertyChanged("Item[]");
            this.OnCollectionReset();
        }

        protected override void InsertItem(int index, T item)
        {
            this.CheckReentrancy();
            base.InsertItem(index, item);
            this.OnPropertyChanged("Count");
            this.OnPropertyChanged("Item[]");
            this.OnCollectionChanged(NotificationCollectionChangedAction.Add, item, index);
        }

        protected override void SetItem(int index, T item)
        {
            this.CheckReentrancy();
            T oldItem = base[index];
            base.SetItem(index, item);
            this.OnPropertyChanged("Item[]");
            this.OnCollectionChanged(NotificationCollectionChangedAction.Replace, oldItem, item, index);
        }

        protected override void RemoveItem(int index)
        {
            this.CheckReentrancy();
            T item = base[index];
            base.RemoveItem(index);
            this.OnPropertyChanged("Count");
            this.OnPropertyChanged("Item[]");
            this.OnCollectionChanged(NotificationCollectionChangedAction.Remove, item, index);
        }

        protected virtual void MoveItem(int oldIndex, int newIndex)
        {
            this.CheckReentrancy();
            T item = base[oldIndex];
            base.RemoveItem(oldIndex);
            base.InsertItem(newIndex, item);
            this.OnPropertyChanged("Item[]");
            this.OnCollectionChanged(NotificationCollectionChangedAction.Move, item, newIndex, oldIndex);
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, e);
            }
        }

        #endregion

        #region Private Members

        private void CopyFrom(IEnumerable<T> collection)
        {
            IList<T> items = base.Items;
            if ((collection != null) && (items != null))
            {
                using (IEnumerator<T> enumerator = collection.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        items.Add(enumerator.Current);
                    }
                }
            }
        }

        private void OnCollectionChanged(NotificationCollectionChangedAction action, object item, int index)
        {
            this.OnCollectionChanged(new NotificationCollectionChangedEventArgs(action, item, index));
        }

        private void OnCollectionChanged(NotificationCollectionChangedAction action, object item, int index, int oldIndex)
        {
            this.OnCollectionChanged(new NotificationCollectionChangedEventArgs(action, item, index, oldIndex));
        }

        private void OnCollectionChanged(NotificationCollectionChangedAction action, object oldItem, object newItem, int index)
        {
            this.OnCollectionChanged(new NotificationCollectionChangedEventArgs(action, newItem, oldItem, index));
        }

        private void OnCollectionReset()
        {
            this.OnCollectionChanged(new NotificationCollectionChangedEventArgs(NotificationCollectionChangedAction.Reset));
        }

        private void OnPropertyChanged(string propertyName)
        {
            this.OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        #endregion


        #region Simple Monitor

        [Serializable]
        private class SimpleMonitor : IDisposable
        {
            // Fields
            private int busyCount;

            // ConstruConstructor
            public SimpleMonitor()
            {
            }

            public void Dispose()
            {
                busyCount--;
            }
            public void Enter()
            {
                busyCount++;
            }

            // Properties
            public bool Busy
            {
                get { return busyCount > 0; }
            }
        }

        #endregion

        #region INotifyPropertyChanged Members

        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
        {
            add
            {
                this.PropertyChanged = (PropertyChangedEventHandler)Delegate.Combine(this.PropertyChanged, value);
            }
            remove
            {
                this.PropertyChanged = (PropertyChangedEventHandler)Delegate.Remove(this.PropertyChanged, value);
            }
        }

        #endregion
    }

    public delegate void NotificationCollectionChangedEventHandler(object sender, NotificationCollectionChangedEventArgs e);

    [Serializable]
    public class NotificationCollectionChangedEventArgs : EventArgs
    {
        #region Private Data

        private NotificationCollectionChangedAction action;
        private IList newItems;
        private int newStartingIndex;
        private IList oldItems;
        private int oldStartingIndex;

        #endregion

        #region ConstruConstructors

        public NotificationCollectionChangedEventArgs(NotificationCollectionChangedAction action)
        {
            this.newStartingIndex = -1;
            this.oldStartingIndex = -1;
            if (action != NotificationCollectionChangedAction.Reset)
            {
                throw new ArgumentException("Wrong Action For ConstruConstructor: " + NotificationCollectionChangedAction.Reset, "action");
            }
            this.InitializeAdd(action, null, -1);
        }

        public NotificationCollectionChangedEventArgs(NotificationCollectionChangedAction action, IList changedItems)
        {
            this.newStartingIndex = -1;
            this.oldStartingIndex = -1;
            if (((action != NotificationCollectionChangedAction.Add) && (action != NotificationCollectionChangedAction.Remove)) && (action != NotificationCollectionChangedAction.Reset))
            {
                throw new ArgumentException("Must Be Reset Add Or Remove Action For ConstruConstructor", "action");
            }
            if (action == NotificationCollectionChangedAction.Reset)
            {
                if (changedItems != null)
                {
                    throw new ArgumentException("ResetActionRequiresNullItem", "action");
                }
                this.InitializeAdd(action, null, -1);
            }
            else
            {
                if (changedItems == null)
                {
                    throw new ArgumentNullException("changedItems");
                }
                this.InitializeAddOrRemove(action, changedItems, -1);
            }
        }

        public NotificationCollectionChangedEventArgs(NotificationCollectionChangedAction action, object changedItem)
        {
            this.newStartingIndex = -1;
            this.oldStartingIndex = -1;
            if (((action != NotificationCollectionChangedAction.Add) && (action != NotificationCollectionChangedAction.Remove)) && (action != NotificationCollectionChangedAction.Reset))
            {
                throw new ArgumentException("Must Be Reset Add Or Remove Action For ConstruConstructor", "action");
            }
            if (action == NotificationCollectionChangedAction.Reset)
            {
                if (changedItem != null)
                {
                    throw new ArgumentException("Reset Action Requires Null Item", "action");
                }
                this.InitializeAdd(action, null, -1);
            }
            else
            {
                this.InitializeAddOrRemove(action, new object[] { changedItem }, -1);
            }
        }

        public NotificationCollectionChangedEventArgs(NotificationCollectionChangedAction action, IList newItems, IList oldItems)
        {
            this.newStartingIndex = -1;
            this.oldStartingIndex = -1;
            if (action != NotificationCollectionChangedAction.Replace)
            {
                throw new ArgumentException("Wrong Action For ConstruConstructor: " + NotificationCollectionChangedAction.Replace, "action");
            }
            if (newItems == null)
            {
                throw new ArgumentNullException("newItems");
            }
            if (oldItems == null)
            {
                throw new ArgumentNullException("oldItems");
            }
            this.InitializeMoveOrReplace(action, newItems, oldItems, -1, -1);
        }

        public NotificationCollectionChangedEventArgs(NotificationCollectionChangedAction action, IList changedItems, int startingIndex)
        {
            this.newStartingIndex = -1;
            this.oldStartingIndex = -1;
            if (((action != NotificationCollectionChangedAction.Add) && (action != NotificationCollectionChangedAction.Remove)) && (action != NotificationCollectionChangedAction.Reset))
            {
                throw new ArgumentException("Must Be Reset Add Or Remove Action For ConstruConstructor", "action");
            }
            if (action == NotificationCollectionChangedAction.Reset)
            {
                if (changedItems != null)
                {
                    throw new ArgumentException("Reset Action Requires Null Item", "action");
                }
                if (startingIndex != -1)
                {
                    throw new ArgumentException("Reset Action Requires (Index - 1)", "action");
                }
                this.InitializeAdd(action, null, -1);
            }
            else
            {
                if (changedItems == null)
                {
                    throw new ArgumentNullException("changedItems");
                }
                if (startingIndex < -1)
                {
                    throw new ArgumentException("Index Cannot Be Negative", "startingIndex");
                }
                this.InitializeAddOrRemove(action, changedItems, startingIndex);
            }
        }

        public NotificationCollectionChangedEventArgs(NotificationCollectionChangedAction action, object changedItem, int index)
        {
            this.newStartingIndex = -1;
            this.oldStartingIndex = -1;
            if (((action != NotificationCollectionChangedAction.Add) && (action != NotificationCollectionChangedAction.Remove)) && (action != NotificationCollectionChangedAction.Reset))
            {
                throw new ArgumentException("Must Be Reset Add Or Remove Action For ConstruConstructor", "action");
            }
            if (action == NotificationCollectionChangedAction.Reset)
            {
                if (changedItem != null)
                {
                    throw new ArgumentException("Reset Action Requires Null Item", "action");
                }
                if (index != -1)
                {
                    throw new ArgumentException("Reset Action Requires (Index - 1)", "action");
                }
                this.InitializeAdd(action, null, -1);
            }
            else
            {
                this.InitializeAddOrRemove(action, new object[] { changedItem }, index);
            }
        }

        public NotificationCollectionChangedEventArgs(NotificationCollectionChangedAction action, object newItem, object oldItem)
        {
            this.newStartingIndex = -1;
            this.oldStartingIndex = -1;
            if (action != NotificationCollectionChangedAction.Replace)
            {
                throw new ArgumentException("Wrong Action For ConstruConstructor: " + NotificationCollectionChangedAction.Replace, "action");
            }
            this.InitializeMoveOrReplace(action, new object[] { newItem }, new object[] { oldItem }, -1, -1);
        }

        public NotificationCollectionChangedEventArgs(NotificationCollectionChangedAction action, IList newItems, IList oldItems, int startingIndex)
        {
            this.newStartingIndex = -1;
            this.oldStartingIndex = -1;
            if (action != NotificationCollectionChangedAction.Replace)
            {
                throw new ArgumentException("Wrong Action For Constructor: " + NotificationCollectionChangedAction.Replace , "action");
            }
            if (newItems == null)
            {
                throw new ArgumentNullException("newItems");
            }
            if (oldItems == null)
            {
                throw new ArgumentNullException("oldItems");
            }
            this.InitializeMoveOrReplace(action, newItems, oldItems, startingIndex, startingIndex);
        }

        public NotificationCollectionChangedEventArgs(NotificationCollectionChangedAction action, IList changedItems, int index, int oldIndex)
        {
            this.newStartingIndex = -1;
            this.oldStartingIndex = -1;
            if (action != NotificationCollectionChangedAction.Move)
            {
                throw new ArgumentException("Wrong Action For Constructor: " + NotificationCollectionChangedAction.Move , "action");
            }
            if (index < 0)
            {
                throw new ArgumentException("Index Cannot Be Negative", "index");
            }
            this.InitializeMoveOrReplace(action, changedItems, changedItems, index, oldIndex);
        }

        public NotificationCollectionChangedEventArgs(NotificationCollectionChangedAction action, object changedItem, int index, int oldIndex)
        {
            this.newStartingIndex = -1;
            this.oldStartingIndex = -1;
            if (action != NotificationCollectionChangedAction.Move)
            {
                throw new ArgumentException("Wrong Action For Constructor: " + NotificationCollectionChangedAction.Move , "action");
            }
            if (index < 0)
            {
                throw new ArgumentException("Index Cannot Be Negative", "index");
            }
            object[] newItems = new object[] { changedItem };
            this.InitializeMoveOrReplace(action, newItems, newItems, index, oldIndex);
        }

        public NotificationCollectionChangedEventArgs(NotificationCollectionChangedAction action, object newItem, object oldItem, int index)
        {
            this.newStartingIndex = -1;
            this.oldStartingIndex = -1;
            if (action != NotificationCollectionChangedAction.Replace)
            {
                throw new ArgumentException("Wrong Action For Constructor: " + NotificationCollectionChangedAction.Replace , "action");
            }
            this.InitializeMoveOrReplace(action, new object[] { newItem }, new object[] { oldItem }, index, index);
        }

        #endregion

        #region Public Members

        public NotificationCollectionChangedAction Action
        {
            get
            {
                return this.action;
            }
        }

        public IList NewItems
        {
            get
            {
                return this.newItems;
            }
        }

        public int NewStartingIndex
        {
            get
            {
                return this.newStartingIndex;
            }
        }

        public IList OldItems
        {
            get
            {
                return this.oldItems;
            }
        }

        public int OldStartingIndex
        {
            get
            {
                return this.oldStartingIndex;
            }
        }

        #endregion

        #region Private Members

        private void InitializeAdd(NotificationCollectionChangedAction action, IList newItems, int newStartingIndex)
        {
            this.action = action;
            this.newItems = (newItems == null) ? null : ArrayList.ReadOnly(newItems);
            this.newStartingIndex = newStartingIndex;
        }

        private void InitializeAddOrRemove(NotificationCollectionChangedAction action, IList changedItems, int startingIndex)
        {
            if (action == NotificationCollectionChangedAction.Add)
            {
                this.InitializeAdd(action, changedItems, startingIndex);
            }
            else if (action == NotificationCollectionChangedAction.Remove)
            {
                this.InitializeRemove(action, changedItems, startingIndex);
            }
            else
            {
                Debug.Assert(false, "Unsupported action: {0}", action.ToString());
            }
        }

        private void InitializeMoveOrReplace(NotificationCollectionChangedAction action, IList newItems, IList oldItems, int startingIndex, int oldStartingIndex)
        {
            this.InitializeAdd(action, newItems, startingIndex);
            this.InitializeRemove(action, oldItems, oldStartingIndex);
        }

        private void InitializeRemove(NotificationCollectionChangedAction action, IList oldItems, int oldStartingIndex)
        {
            this.action = action;
            this.oldItems = (oldItems == null) ? null : ArrayList.ReadOnly(oldItems);
            this.oldStartingIndex = oldStartingIndex;
        }

        #endregion
    }

    public enum NotificationCollectionChangedAction
    {
        Add,
        Remove,
        Replace,
        Move,
        Reset
    }
}
