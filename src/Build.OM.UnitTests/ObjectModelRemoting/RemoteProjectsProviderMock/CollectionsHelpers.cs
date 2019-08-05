// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Build.UnitTests.OM.ObjectModelRemoting
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;


    static class CollectionHelpers
    {
        public static ICollection<T> ImportCollection<T, RMock>(this ProjectCollectionLinker importer, IEnumerable<RMock> source)
            where T : class
            where RMock : MockLinkRemoter<T>, new()
        {
            if (source == null) return null;
            // Just copy ...
            List<T> result = new List<T>();
            foreach (var sRemoter in source)
            {
                var s = importer.Import<T, RMock>(s);
                result.Add(s);
            }

            return result;
        }

        public static ICollection<RMock> ExportCollection<T, RMock>(this ProjectCollectionLinker exporter, IEnumerable<T> source)
            where T : class
            where RMock : MockLinkRemoter<T>, new()
        {
            if (source == null) return null;
            // Just copy ...
            List<RMock> result = new List<RMock>();
            foreach (var s in source)
            {
                var sRemoter = exporter.Export<T, RMock>(s);
                result.Add(sRemoter);
            }
            return result;
        }

        public static IDictionary<key, T> ImportDictionary<key, T, RMock>(this ProjectCollectionLinker importer, IDictionary<key, RMock> source)
            where T : class
            where RMock : MockLinkRemoter<T>, new()
        {
            if (source == null) return null;
            // Just copy ...
            Dictionary<key, T> result = new Dictionary<key, T>();
            foreach (var sRemoter in source)
            {
                var value = importer.Import<T, RMock>(sRemoter.Value);
                result.Add(sRemoter.Key, value);
            }

            return result;
        }

        public static IDictionary<key, RMock> ExportDictionary<key, T, RMock>(this ProjectCollectionLinker exporter, IDictionary<key, T> source)
            where T : class
            where RMock : MockLinkRemoter<T>, new()
        {
            if (source == null) return null;
            // Just copy ...
            Dictionary<key, RMock> result = new Dictionary<key, RMock>();
            foreach (var s in source)
            {
                var valueRemoter = exporter.Export<T, RMock>(s.Value);
                result.Add(s.Key, valueRemoter);
            }

            return result;
        }
    }
}
