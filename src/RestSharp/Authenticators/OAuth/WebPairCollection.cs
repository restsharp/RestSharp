//   Copyright (c) .NET Foundation and Contributors
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License. 

using System.Collections;

namespace RestSharp.Authenticators.OAuth; 

class WebPairCollection : IList<WebPair> {
    readonly List<WebPair> _parameters = [];

    public IEnumerator<WebPair> GetEnumerator() => _parameters.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void Add(WebPair parameter) => _parameters.Add(parameter);

    public void AddRange(IEnumerable<WebPair> collection) => AddCollection(collection);

    public void Add(string name, string value) => Add(new WebPair(name, value));
    
    public WebPairCollection AddNotEmpty(string name, string? value, bool encode = false) {
        if (value != null)
            Add(new WebPair(name, value, encode));
        return this;
    }

    public void Clear() => _parameters.Clear();

    public bool Contains(WebPair parameter) => _parameters.Contains(parameter);

    public void CopyTo(WebPair[] parametersArray, int arrayIndex) => _parameters.CopyTo(parametersArray, arrayIndex);

    public bool Remove(WebPair parameter) => _parameters.Remove(parameter);

    public int Count => _parameters.Count;

    public bool IsReadOnly => false;

    public int IndexOf(WebPair parameter) => _parameters.IndexOf(parameter);

    public void Insert(int index, WebPair parameter) => _parameters.Insert(index, parameter);

    public void RemoveAt(int index) => _parameters.RemoveAt(index);

    public WebPair this[int index] {
        get => _parameters[index];
        set => _parameters[index] = value;
    }

    void AddCollection(IEnumerable<WebPair> collection)
        => _parameters.AddRange(collection.Select(parameter => new WebPair(parameter.Name, parameter.Value)));
}