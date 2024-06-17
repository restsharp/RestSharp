// Copyright (c) .NET Foundation and Contributors
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

namespace SourceGenerator;

static class Extensions {
    public static IEnumerable<ClassDeclarationSyntax> FindClasses(this Compilation compilation, Func<ClassDeclarationSyntax, bool> predicate)
        => compilation.SyntaxTrees
            .Select(tree => compilation.GetSemanticModel(tree))
            .SelectMany(model => model.SyntaxTree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>())
            .Where(predicate);

    public static IEnumerable<ClassDeclarationSyntax> FindAnnotatedClass(this Compilation compilation, string attributeName, bool strict) {
        return compilation.FindClasses(
            syntax => syntax.AttributeLists.Any(list => list.Attributes.Any(CheckAttribute))
        );

        bool CheckAttribute(AttributeSyntax attr) {
            var name = attr.Name.ToString();
            return strict ? name == attributeName : name.StartsWith(attributeName);
        }
    }

    public static IEnumerable<ITypeSymbol> GetBaseTypesAndThis(this ITypeSymbol type) {
        var current = type;

        while (current != null) {
            yield return current;

            current = current.BaseType;
        }
    }
}