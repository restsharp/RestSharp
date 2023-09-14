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

using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace SourceGenerator;

[Generator]
public class ImmutableGenerator : ISourceGenerator {
    public void Initialize(GeneratorInitializationContext context) { }

    public void Execute(GeneratorExecutionContext context) {
        var compilation = context.Compilation;

        var mutableClasses = compilation.SyntaxTrees
            .Select(tree => compilation.GetSemanticModel(tree))
            .SelectMany(model => model.SyntaxTree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>())
            .Where(syntax => syntax.AttributeLists.Any(list => list.Attributes.Any(attr => attr.Name.ToString() == "GenerateImmutable")));

        foreach (var mutableClass in mutableClasses) {
            var immutableClass = GenerateImmutableClass(mutableClass, compilation);
            context.AddSource($"ReadOnly{mutableClass.Identifier.Text}.cs", SourceText.From(immutableClass, Encoding.UTF8));
        }
    }

    static string GenerateImmutableClass(TypeDeclarationSyntax mutableClass, Compilation compilation) {
        var containingNamespace = compilation.GetSemanticModel(mutableClass.SyntaxTree).GetDeclaredSymbol(mutableClass)!.ContainingNamespace;

        var namespaceName = containingNamespace.ToDisplayString();

        var className = mutableClass.Identifier.Text;

        var usings = mutableClass.SyntaxTree.GetCompilationUnitRoot().Usings.Select(u => u.ToString());

        var properties = GetDefinitions(SyntaxKind.SetKeyword)
            .Select(prop => $"    public {prop.Type} {prop.Identifier.Text} {{ get; }}")
            .ToArray();

        var props = GetDefinitions(SyntaxKind.SetKeyword).ToArray();

        const string argName = "inner";
        var mutableProperties = props
            .Select(prop => $"        {prop.Identifier.Text} = {argName}.{prop.Identifier.Text};");

        var constructor = $$"""
                                public ReadOnly{{className}}({{className}} {{argName}}) {
                            {{string.Join("\n", mutableProperties)}}
                                    CopyAdditionalProperties({{argName}});
                                }
                            """;

        const string template = @"{Usings}

namespace {Namespace};

public partial class ReadOnly{ClassName} {
{Constructor}

    partial void CopyAdditionalProperties({ClassName} {ArgName}); 

{Properties}
}";

        var code = template
            .Replace("{Usings}", string.Join("\n", usings))
            .Replace("{Namespace}", namespaceName)
            .Replace("{ClassName}", className)
            .Replace("{Constructor}", constructor)
            .Replace("{ArgName}", argName)
            .Replace("{Properties}", string.Join("\n", properties));

        return code;

        IEnumerable<PropertyDeclarationSyntax> GetDefinitions(SyntaxKind kind)
            => mutableClass.Members
                .OfType<PropertyDeclarationSyntax>()
                .Where(
                    prop =>
                        prop.AccessorList!.Accessors.Any(accessor => accessor.Keyword.IsKind(kind)) && prop.AttributeLists.All(list => list.Attributes.All(attr => attr.Name.ToString() != "Exclude"))
                );
    }
}
