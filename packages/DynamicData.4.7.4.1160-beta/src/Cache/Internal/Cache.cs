﻿using System;
using System.Collections.Generic;
using DynamicData.Kernel;

namespace DynamicData.Internal
{
    internal class Cache<TObject, TKey> : ICache<TObject, TKey>
    {
        private Dictionary<TKey, TObject> _data;

        public int Count => _data.Count;
        public IEnumerable<KeyValuePair<TKey, TObject>> KeyValues => _data;
        public IEnumerable<TObject> Items => _data.Values;
        public IEnumerable<TKey> Keys => _data.Keys;

        public Cache()
        {
            _data = new Dictionary<TKey, TObject>();
        }

        public void Clone(IChangeSet<TObject, TKey> changes)
        {
            if (changes == null) throw new ArgumentNullException(nameof(changes));

            //for efficiency resize dictionary to initial batch size
            if (_data.Count == 0)
                _data = new Dictionary<TKey, TObject>(changes.Count);

            foreach (var item in changes)
            {
                switch (item.Reason)
                {
                    case ChangeReason.Update:
                    case ChangeReason.Add:
                    {
                        _data[item.Key] = item.Current;
                    }
                        break;
                    case ChangeReason.Remove:
                        _data.Remove(item.Key);
                        break;
                }
            }
        }

        public Optional<TObject> Lookup(TKey key)
        {
            return _data.Lookup(key);
        }

        public void AddOrUpdate(TObject item, TKey key)
        {
            _data[key] = item;
        }

        public void Remove(TKey key)
        {
            if (_data.ContainsKey(key))
                _data.Remove(key);
        }

        public void Clear()
        {
            _data.Clear();
        }
    }
}
