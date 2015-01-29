using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.Foundation.Collections;
using Windows.Foundation.Metadata;
using Windows.UI.Xaml.Data;

namespace DocDirect
{
    public static partial class AppAid
    {
        public static async void Forget(this System.Threading.Tasks.Task task) { await task; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Forget(this Windows.Foundation.IAsyncInfo asyncInfo) { }
    }
    [WebHostHidden]
    public abstract class ObservableList<T> : IObservableVector<T>
    {
        private readonly List<T> m_list = new List<T>();

        private sealed class VectorChangedEventArgs : IVectorChangedEventArgs
        {
            public VectorChangedEventArgs(UInt32 index, CollectionChange change)
            {
                Index = index;
                CollectionChange = change;
            }
            public CollectionChange CollectionChange { get; private set; }
            public UInt32 Index { get; private set; }
        }

        protected virtual void OnVectorChanged(Int32 index, CollectionChange change)
        {
            var t = VectorChanged;
            if (t == null) return;
            t(this, new VectorChangedEventArgs((UInt32)index, change));
        }

        #region IList
        public T this[int index]
        {
            get { return m_list[index]; }
            set { m_list[index] = value; OnVectorChanged(index, CollectionChange.ItemChanged); }
        }
        public int IndexOf(T item) { return m_list.IndexOf(item); }
        public void Insert(int index, T item) { m_list.Insert(index, item); OnVectorChanged(index, CollectionChange.ItemInserted); }
        public void RemoveAt(int index) { m_list.RemoveAt(index); OnVectorChanged(index, CollectionChange.ItemRemoved); }
        #endregion

        #region ICollection
        public Int32 Count { get { return m_list.Count; } }
        public Boolean IsReadOnly { get { return false; } }
        public void Add(T item) { m_list.Insert(Count, item); }
        public void Clear() { m_list.Clear(); OnVectorChanged(0, CollectionChange.Reset); }
        public Boolean Contains(T item) { return m_list.Contains(item); }
        public void CopyTo(T[] array, int arrayIndex) { m_list.CopyTo(array, arrayIndex); }
        public Boolean Remove(T item)
        {
            Int32 index = m_list.IndexOf(item);
            if (index == -1) return false;
            m_list.RemoveAt(index);
            return true;
        }
        #endregion

        #region IEnumerator
        IEnumerator IEnumerable.GetEnumerator() { return m_list.GetEnumerator(); }
        public IEnumerator<T> GetEnumerator() { return m_list.GetEnumerator(); }
        #endregion

        public event VectorChangedEventHandler<T> VectorChanged;
    }
    [WebHostHidden]
    public abstract class ObservableList : IObservableVector<Object>
    {
        private readonly List<Object> m_list = new List<Object>();

        private sealed class VectorChangedEventArgs : IVectorChangedEventArgs
        {
            public VectorChangedEventArgs(UInt32 index, CollectionChange change)
            {
                Index = index;
                CollectionChange = change;
            }
            public CollectionChange CollectionChange { get; private set; }
            public UInt32 Index { get; private set; }
        }

        protected virtual void OnVectorChanged(Int32 index, CollectionChange change)
        {
            var t = VectorChanged;
            if (t == null) return;
            t(this, new VectorChangedEventArgs((UInt32)index, change));
        }

        #region IList
        public Object this[int index]
        {
            get { return m_list[index]; }
            set { m_list[index] = value; OnVectorChanged(index, CollectionChange.ItemChanged); }
        }
        public int IndexOf(Object item) { return m_list.IndexOf(item); }
        public void Insert(int index, Object item) { m_list.Insert(index, item); OnVectorChanged(index, CollectionChange.ItemInserted); }
        public void RemoveAt(int index) { m_list.RemoveAt(index); OnVectorChanged(index, CollectionChange.ItemRemoved); }
        #endregion

        #region ICollection
        public Int32 Count { get { return m_list.Count; } }
        public Boolean IsReadOnly { get { return false; } }
        public void Add(Object item) { Insert(Count, item); }
        public void Clear() { m_list.Clear(); OnVectorChanged(0, CollectionChange.Reset); }
        public Boolean Contains(Object item) { return m_list.Contains(item); }
        public void CopyTo(Object[] array, int arrayIndex) { m_list.CopyTo(array, arrayIndex); }
        public Boolean Remove(Object item)
        {
            Int32 index = IndexOf(item);
            if (index == -1) return false;
            RemoveAt(index);
            return true;
        }
        #endregion

        #region IEnumerator
        IEnumerator IEnumerable.GetEnumerator() { return m_list.GetEnumerator(); }
        public IEnumerator<Object> GetEnumerator() { return m_list.GetEnumerator(); }
        #endregion

        public event VectorChangedEventHandler<Object> VectorChanged;
    }
}
