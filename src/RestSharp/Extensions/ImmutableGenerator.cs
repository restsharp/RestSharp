//  Copyright (c) .NET Foundation and Contributors
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

using System.Linq.Expressions;
using System.Reflection;

namespace RestSharp.Extensions;

static class ImmutableGenerator {
    static readonly MethodInfo SetPropertyMethod =
        typeof(ImmutableGenerator).GetMethod(nameof(SetProperty), BindingFlags.Static | BindingFlags.NonPublic)!;

    static void SetProperty<T>(T obj, PropertyInfo property, object value) => property.SetValue(obj, (T)value);

    public static Func<TMutable, TImmutable> CreateImmutableFunc<TMutable, TImmutable>()
        where TMutable : class
        where TImmutable : class, new()
    {
        var mutableType   = typeof(TMutable);
        var immutableType = typeof(TImmutable);

        var parameter = Expression.Parameter(mutableType, "mutable");

        var bindings = mutableType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(property =>
                Expression.Bind(
                    immutableType.GetProperty(property.Name)!,
                    Expression.Property(parameter, property)));

        var body = Expression.MemberInit(Expression.New(immutableType), bindings);

        var lambda = Expression.Lambda<Func<TMutable, TImmutable>>(body, parameter);

        return lambda.Compile();
    }
}
