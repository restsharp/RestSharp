using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace RestSharp.Authenticators.OAuth
{
    internal class WebPairCollection : IList<WebPair>
    {
        private List<WebPair> parameters;

        public WebPairCollection(IEnumerable<WebPair> parameters)
        {
            this.parameters = new List<WebPair>(parameters);
        }

        public WebPairCollection(NameValueCollection collection)
            : this()
        {
            AddCollection(collection);
        }

        public WebPairCollection(IDictionary<string, string> collection)
            : this()
        {
            AddCollection(collection);
        }

        public WebPairCollection()
        {
            parameters = new List<WebPair>(0);
        }

        public WebPairCollection(int capacity)
        {
            parameters = new List<WebPair>(capacity);
        }

        public virtual WebPair this[string name]
        {
            get { return this.SingleOrDefault(p => p.Name.Equals(name)); }
        }

        public virtual IEnumerable<string> Names
        {
            get { return parameters.Select(p => p.Name); }
        }

        public virtual IEnumerable<string> Values
        {
            get { return parameters.Select(p => p.Value); }
        }

        public virtual void AddRange(NameValueCollection collection)
        {
            AddCollection(collection);
        }

        private void AddCollection(NameValueCollection collection)
        {
            parameters.AddRange(collection.AllKeys.Select(key => new WebPair(key, collection[key])));
        }

        public void AddCollection(IDictionary<string, string> collection)
        {
            parameters.AddRange(collection.Keys.Select(key => new WebPair(key, collection[key])));
        }

        private void AddCollection(IEnumerable<WebPair> collection)
        {
            parameters.AddRange(collection.Select(parameter => new WebPair(parameter.Name, parameter.Value)));
        }

        public virtual void AddRange(WebPairCollection collection)
        {
            AddCollection(collection);
        }

        public virtual void AddRange(IEnumerable<WebPair> collection)
        {
            AddCollection(collection);
        }

        public virtual void Sort(Comparison<WebPair> comparison)
        {
            var sorted = new List<WebPair>(parameters);

            sorted.Sort(comparison);

            parameters = sorted;
        }

        public virtual bool RemoveAll(IEnumerable<WebPair> parametersToRemove)
        {
            var array = parametersToRemove.ToArray();
            var success = array.Aggregate(true, (current, parameter) => current & parameters.Remove(parameter));

            return success && array.Length > 0;
        }

        public virtual void Add(string name, string value)
        {
            var pair = new WebPair(name, value);

            parameters.Add(pair);
        }

        #region IList<WebParameter> Members

        public virtual IEnumerator<WebPair> GetEnumerator()
        {
            return parameters.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public virtual void Add(WebPair parameter)
        {
            parameters.Add(parameter);
        }

        public virtual void Clear()
        {
            parameters.Clear();
        }

        public virtual bool Contains(WebPair parameter)
        {
            return parameters.Contains(parameter);
        }

        public virtual void CopyTo(WebPair[] parametersArray, int arrayIndex)
        {
            parameters.CopyTo(parametersArray, arrayIndex);
        }

        public virtual bool Remove(WebPair parameter)
        {
            return parameters.Remove(parameter);
        }

        public virtual int Count => parameters.Count;

        public virtual bool IsReadOnly => false;

        public virtual int IndexOf(WebPair parameter)
        {
            return parameters.IndexOf(parameter);
        }

        public virtual void Insert(int index, WebPair parameter)
        {
            parameters.Insert(index, parameter);
        }

        public virtual void RemoveAt(int index)
        {
            parameters.RemoveAt(index);
        }

        public virtual WebPair this[int index]
        {
            get => parameters[index];
            set => parameters[index] = value;
        }
    }
}