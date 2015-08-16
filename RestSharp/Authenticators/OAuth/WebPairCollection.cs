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

        public virtual WebPair this[string name]
        {
            get { return this.SingleOrDefault(p => p.Name.Equals(name)); }
        }

        public virtual IEnumerable<string> Names
        {
            get { return this.parameters.Select(p => p.Name); }
        }

        public virtual IEnumerable<string> Values
        {
            get { return this.parameters.Select(p => p.Value); }
        }

        public WebPairCollection(IEnumerable<WebPair> parameters)
        {
            this.parameters = new List<WebPair>(parameters);
        }

#if !WINDOWS_PHONE && !SILVERLIGHT
        public WebPairCollection(NameValueCollection collection)
            : this()
        {
            this.AddCollection(collection);
        }

        public virtual void AddRange(NameValueCollection collection)
        {
            this.AddCollection(collection);
        }

        private void AddCollection(NameValueCollection collection)
        {
            this.parameters.AddRange(collection.AllKeys.Select(key => new WebPair(key, collection[key])));
        }
#endif

        public WebPairCollection(IDictionary<string, string> collection)
            : this()
        {
            this.AddCollection(collection);
        }

        public void AddCollection(IDictionary<string, string> collection)
        {
            this.parameters.AddRange(collection.Keys.Select(key => new WebPair(key, collection[key])));
        }

        public WebPairCollection()
        {
            this.parameters = new List<WebPair>(0);
        }

        public WebPairCollection(int capacity)
        {
            this.parameters = new List<WebPair>(capacity);
        }

        private void AddCollection(IEnumerable<WebPair> collection)
        {
            this.parameters.AddRange(collection.Select(parameter => new WebPair(parameter.Name, parameter.Value)));
        }

        public virtual void AddRange(WebPairCollection collection)
        {
            this.AddCollection(collection);
        }

        public virtual void AddRange(IEnumerable<WebPair> collection)
        {
            this.AddCollection(collection);
        }

        public virtual void Sort(Comparison<WebPair> comparison)
        {
            List<WebPair> sorted = new List<WebPair>(this.parameters);

            sorted.Sort(comparison);

            this.parameters = sorted;
        }

        public virtual bool RemoveAll(IEnumerable<WebPair> parametersToRemove)
        {
            WebPair[] array = parametersToRemove.ToArray();
            bool success = array.Aggregate(true, (current, parameter) => current & this.parameters.Remove(parameter));

            return success && array.Length > 0;
        }

        public virtual void Add(string name, string value)
        {
            WebPair pair = new WebPair(name, value);

            this.parameters.Add(pair);
        }

        #region IList<WebParameter> Members

        public virtual IEnumerator<WebPair> GetEnumerator()
        {
            return this.parameters.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public virtual void Add(WebPair parameter)
        {
            this.parameters.Add(parameter);
        }

        public virtual void Clear()
        {
            this.parameters.Clear();
        }

        public virtual bool Contains(WebPair parameter)
        {
            return this.parameters.Contains(parameter);
        }

        public virtual void CopyTo(WebPair[] parametersArray, int arrayIndex)
        {
            this.parameters.CopyTo(parametersArray, arrayIndex);
        }

        public virtual bool Remove(WebPair parameter)
        {
            return this.parameters.Remove(parameter);
        }

        public virtual int Count
        {
            get { return this.parameters.Count; }
        }

        public virtual bool IsReadOnly
        {
            get { return false; }
        }

        public virtual int IndexOf(WebPair parameter)
        {
            return this.parameters.IndexOf(parameter);
        }

        public virtual void Insert(int index, WebPair parameter)
        {
            this.parameters.Insert(index, parameter);
        }

        public virtual void RemoveAt(int index)
        {
            this.parameters.RemoveAt(index);
        }

        public virtual WebPair this[int index]
        {
            get { return this.parameters[index]; }
            set { this.parameters[index] = value; }
        }

        #endregion
    }
}
