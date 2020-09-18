//  Copyright © 2009-2020 John Sheehan, Andrew Young, Alexey Zimarev and RestSharp community
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using JetBrains.Annotations;

namespace RestSharp
{
    /// <summary>
    /// Read only collection with internal visible Add/Remove methods
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PublicReadOnlyCollection<T> : ReadOnlyCollection<T>
    {
        public PublicReadOnlyCollection() : this(new List<T>()) { }
        public PublicReadOnlyCollection([NotNull] IList<T> list) : base(list) { }

        /// <inheritdoc cref="T:System.Collections.Generic.IList`1"/>
        internal void Add(T item) => Items.Add(item);

        /// <inheritdoc cref="T:System.Collections.Generic.IList`1"/>
        internal bool Remove(T item) => Items.Remove(item);

        /// <summary>Retrieves all the elements that match the conditions defined by the specified predicate.</summary>
        /// <param name="match">The <see cref="T:System.Predicate`1" /> delegate that defines the conditions of the elements to search for.</param>
        /// <returns>A <see cref="T:System.Collections.Generic.List`1" /> containing all the elements that match the conditions defined by the specified predicate, if found; otherwise, an empty <see cref="T:System.Collections.Generic.List`1" />.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="match" /> is <see langword="null" />.</exception>
        public PublicReadOnlyCollection<T> FindAll(Predicate<T> match)
        {
            if (match == null) throw new ArgumentNullException(nameof(match));
            PublicReadOnlyCollection<T> objList = new PublicReadOnlyCollection<T>();
            for (var index = 0; index < Items.Count; ++index)
            {
                if (match(Items[index]))
                    objList.Add(Items[index]);
            }
            return objList;
        }
    }
}