using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace GB28181.Client
{
    /// <summary>
    /// 带通知更改的Dictionary 支持两级
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TRoot">根类型</typeparam>
    /// <typeparam name="TValue">值类型</typeparam>
    public abstract class NotifyChangeDictionaryFixRef<TKey, TRoot, TValue> : IEnumerable<KeyValuePair<TKey, TRoot>>, IEnumerable
    {
        protected ConcurrentDictionary<TKey, TRoot> dit = new ConcurrentDictionary<TKey, TRoot>();
        protected abstract void OnFixAdd(TRoot fixitem);
        protected abstract void OnFixRemove(TRoot fixitem);
        protected abstract TKey GetKey(TValue item);
        protected abstract TRoot NewValue(TValue item);

        protected abstract void ChangeValue(TRoot root, TValue child);
        public virtual void AddOrUpdate(TValue value)
        {
            var root = dit.GetOrAdd(GetKey(value), p =>
            {
                var root = NewValue(value);
                OnFixAdd(root);
                return root;
            });
            ChangeValue(root, value);
        }
        public virtual bool TryRemove(TKey key, out TRoot value)
        {
            if (dit.TryRemove(key, out value))
            {
                OnFixRemove(value);
                return true;
            }
            return false;
        }
        public virtual void ChangeAll(IEnumerable<TValue> lst)
        {
            var keys = dit.Keys.ToList();
            //
            if (lst != null)
            {
                foreach (var item in lst)
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
        public bool TryGetValue(TKey key, out TRoot value) => dit.TryGetValue(key, out value);
        public IEnumerator<KeyValuePair<TKey, TRoot>> GetEnumerator() => dit.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => dit.GetEnumerator();

        public int Count => dit.Count;

    }
}
