using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RestSharp.Authenticators.OAuth
{
    internal class WebPairCollection : IList<WebPair>
    {
        readonly List<WebPair> _parameters = new List<WebPair>();

        public IEnumerator<WebPair> GetEnumerator() => _parameters.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void Add(WebPair parameter) => _parameters.Add(parameter);
        
        public void AddRange(IEnumerable<WebPair> collection) => AddCollection(collection);

        public void Add(string name, string value) => Add(new WebPair(name, value));

        public void Clear() => _parameters.Clear();

        public bool Contains(WebPair parameter) => _parameters.Contains(parameter);

        public void CopyTo(WebPair[] parametersArray, int arrayIndex) => _parameters.CopyTo(parametersArray, arrayIndex);

        public bool Remove(WebPair parameter) => _parameters.Remove(parameter);

        public int Count => _parameters.Count;

        public bool IsReadOnly => false;

        public int IndexOf(WebPair parameter) => _parameters.IndexOf(parameter);

        public void Insert(int index, WebPair parameter) => _parameters.Insert(index, parameter);

        public void RemoveAt(int index) => _parameters.RemoveAt(index);

        public WebPair this[int index]
        {
            get => _parameters[index];
            set => _parameters[index] = value;
        }

        void AddCollection(IEnumerable<WebPair> collection)
            => _parameters.AddRange(collection.Select(parameter => new WebPair(parameter.Name, parameter.Value)));
    }
}