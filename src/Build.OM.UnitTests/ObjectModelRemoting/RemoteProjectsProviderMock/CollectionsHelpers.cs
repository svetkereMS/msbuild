// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Build.UnitTests.OM.ObjectModelRemoting
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;


    internal class ReadOnlyDictionary<key, value> : IDictionary<key, value>
      where value : class
    {
        Dictionary<key, value> inner;

        public value this[key key] { get => inner[key]; set => throw new NotImplementedException(); }

        public ICollection<key> Keys => throw new NotImplementedException();

        public ICollection<value> Values => throw new NotImplementedException();

        public int Count => inner.Count;

        public bool IsReadOnly => true;

        public void Add(key key, value value)
        {
            throw new NotImplementedException();
        }

        public void Add(KeyValuePair<key, value> item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(KeyValuePair<key, value> item)
        {
            return inner.Contains(item);
        }

        public bool ContainsKey(key key)
        {
            return inner.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<key, value>[] array, int arrayIndex)
        {
            if (array != null)
            {
                foreach (var e in inner)
                {
                    if (arrayIndex >= array.Length)
                    {
                        return;
                    }

                    array[arrayIndex] = e;
                    arrayIndex++;
                }
            }
        }

        public IEnumerator<KeyValuePair<key, value>> GetEnumerator()
        {
            return inner.GetEnumerator();
        }

        public bool Remove(key key)
        {
            throw new NotImplementedException();
        }

        public bool Remove(KeyValuePair<key, value> item)
        {
            throw new NotImplementedException();
        }

        public bool TryGetValue(key key, out value value)
        {
            return inner.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)inner).GetEnumerator();
        }
    }


    internal class ReadOnlyLazyRemoteCollection<T> : ICollection<T>
    {
        IReadOnlyCollection<T> roCollection;

        int ICollection<T>.Count => roCollection.Count;

        bool ICollection<T>.IsReadOnly => true;

        void ICollection<T>.Add(T item)
        {
            throw new NotImplementedException();
        }

        void ICollection<T>.Clear()
        {
            throw new NotImplementedException();
        }

        bool ICollection<T>.Contains(T item)
        {
            return roCollection.Contains(item);
        }

        void ICollection<T>.CopyTo(T[] array, int arrayIndex)
        {
            if (array != null)
            {
                foreach (var e in this.roCollection)
                {
                    if (arrayIndex >= array.Length)
                    {
                        return;
                    }

                    array[arrayIndex] = e;
                    arrayIndex++;
                }
            }
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return this.roCollection.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)this.roCollection).GetEnumerator();
        }

        bool ICollection<T>.Remove(T item)
        {
            throw new NotImplementedException();
        }
    }
}
