using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace RestSharp.Authenticators.OAuth
{
    internal class WebPairCollection : IList<WebPair>
    {
        List<WebPair> _parameters = new List<WebPair>(0);

        public virtual IEnumerator<WebPair> GetEnumerator() => _parameters.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public virtual void Add(WebPair parameter) => _parameters.Add(parameter);

        public virtual void Clear() => _parameters.Clear();

        public virtual bool Contains(WebPair parameter) => _parameters.Contains(parameter);

        public virtual void CopyTo(WebPair[] parametersArray, int arrayIndex) => _parameters.CopyTo(parametersArray, arrayIndex);

        public virtual bool Remove(WebPair parameter) => _parameters.Remove(parameter);

        public virtual int Count => _parameters.Count;

        public virtual bool IsReadOnly => false;

        public virtual int IndexOf(WebPair parameter) => _parameters.IndexOf(parameter);

        public virtual void Insert(int index, WebPair parameter) => _parameters.Insert(index, parameter);

        public virtual void RemoveAt(int index) => _parameters.RemoveAt(index);

        public virtual WebPair this[int index]
        {
            get => _parameters[index];
            set => _parameters[index] = value;
        }

        void AddCollection(NameValueCollection collection)
            => _parameters.AddRange(collection.AllKeys.Select(key => new WebPair(key, collection[key])));

        public void AddCollection(IDictionary<string, string> collection)
            => _parameters.AddRange(collection.Keys.Select(key => new WebPair(key, collection[key])));

        void AddCollection(IEnumerable<WebPair> collection)
            => _parameters.AddRange(collection.Select(parameter => new WebPair(parameter.Name, parameter.Value)));

        public void AddRange(IEnumerable<WebPair> collection) => AddCollection(collection);

        public void Sort(Comparison<WebPair> comparison)
        {
            var sorted = new List<WebPair>(_parameters);

            sorted.Sort(comparison);

            _parameters = sorted;
        }

        public void Add(string name, string value) => Add(new WebPair(name, value));
    }
}