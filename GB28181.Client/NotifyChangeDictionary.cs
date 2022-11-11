using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace GB28181.Client
{
    public abstract class NotifyChangeDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable
    {
        protected ConcurrentDictionary<TKey, TValue> dit = new ConcurrentDictionary<TKey, TValue>();
        protected abstract void OnChannelItemAdd(TValue item);
        protected abstract void OnChannelItemUpdate(TValue old, TValue item);
        protected abstract void OnChannelItemRemove(TValue item);
        protected abstract TKey GetKey(TValue item);
        public virtual TValue AddOrUpdate(TValue value)
        {
            return dit.AddOrUpdate(GetKey(value), p =>
             {
                 OnChannelItemAdd(value);
                 return value;
             }, (p, q) =>
             {
                 OnChannelItemUpdate(q, value);
                 return value;
             });
        }
        public virtual bool TryRemove(TKey key, out TValue value)
        {
            if (dit.TryRemove(key, out value))
            {
                OnChannelItemRemove(value);
                return true;
            }
            return false;
        }
        public virtual void ChangeAll(IEnumerable<TValue> deviceList)
        {
            var keys = dit.Keys.ToList();
            //
            if (deviceList != null)
            {
                foreach (var item in deviceList)
                {
                    var key = GetKey(item);
                    AddOrUpdate(item);
                    keys.Remove(key);
                }
            }
            //移除
            foreach (var key in keys)
            {
                TryRemove(key, out var item);
            }
        }
        public bool TryGetValue(TKey key, out TValue value) => dit.TryGetValue(key, out value);
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => dit.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => dit.GetEnumerator();

        public int Count => dit.Count;

    }
}
