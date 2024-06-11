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

[Generator]
public class InheritedCloneGenerator : ISourceGenerator {
    const string AttributeName = "GenerateClone";

    public void Initialize(GeneratorInitializationContext context) { }

    public void Execute(GeneratorExecutionContext context) {
        var compilation = context.Compilation;

        var candidates = compilation.FindAnnotatedClass(AttributeName, false);

        foreach (var candidate in candidates) {
            var semanticModel      = compilation.GetSemanticModel(candidate.SyntaxTree);
            var genericClassSymbol = semanticModel.GetDeclaredSymbol(candidate);
            if (genericClassSymbol == null) continue;

            // Get the method name from the attribute Name argument
            var attributeData = genericClassSymbol.GetAttributes().FirstOrDefault(a => a.AttributeClass?.Name == $"{AttributeName}Attribute");
            var methodName    = (string)attributeData.NamedArguments.FirstOrDefault(arg => arg.Key == "Name").Value.Value;

            // Get the generic argument type where properties need to be copied from
            var attributeSyntax = candidate.AttributeLists
                .SelectMany(l => l.Attributes)
                .FirstOrDefault(a => a.Name.ToString().StartsWith(AttributeName));
            if (attributeSyntax == null) continue; // This should never happen

            var typeArgumentSyntax = ((GenericNameSyntax)attributeSyntax.Name).TypeArgumentList.Arguments[0];
            var typeSymbol         = (INamedTypeSymbol)semanticModel.GetSymbolInfo(typeArgumentSyntax).Symbol;

            var code = GenerateMethod(candidate, genericClassSymbol, typeSymbol, methodName);
            context.AddSource($"{genericClassSymbol.Name}.Clone.g.cs", SourceText.From(code, Encoding.UTF8));
        }
    }

    static string GenerateMethod(
        TypeDeclarationSyntax classToExtendSyntax,
        INamedTypeSymbol      classToExtendSymbol,
        INamedTypeSymbol      classToClone,
        string                methodName
    ) {
        var namespaceName         = classToExtendSymbol.ContainingNamespace.ToDisplayString();
        var className             = classToExtendSyntax.Identifier.Text;
        var genericTypeParameters = string.Join(", ", classToExtendSymbol.TypeParameters.Select(tp => tp.Name));
        var classDeclaration      = classToExtendSymbol.TypeParameters.Length > 0 ? $"{className}<{genericTypeParameters}>" : className;

        var all    = classToClone.GetBaseTypesAndThis();
        var props  = all.SelectMany(x => x.GetMembers().OfType<IPropertySymbol>()).ToArray();
        var usings = classToExtendSyntax.SyntaxTree.GetCompilationUnitRoot().Usings.Select(u => u.ToString());

        var constructorParams     = classToExtendSymbol.Constructors.First().Parameters.ToArray();
        var constructorArgs       = string.Join(", ", constructorParams.Select(p => $"original.{GetPropertyName(p.Name, props)}"));
        var constructorParamNames = constructorParams.Select(p => p.Name).ToArray();

        var properties = props
            // ReSharper disable once PossibleUnintendedLinearSearchInSet
            .Where(prop => !constructorParamNames.Contains(prop.Name, StringComparer.OrdinalIgnoreCase) && prop.SetMethod != null)
            .Select(prop => $"            {prop.Name} = original.{prop.Name},")
            .ToArray();

        const string template = """
                                {Usings}

                                namespace {Namespace};

                                public partial class {ClassDeclaration} {
                                    public static {ClassDeclaration} {MethodName}({OriginalClassName} original)
                                        => new {ClassDeclaration}({ConstructorArgs}) {
                                {Properties}
                                        };
                                }
                                """;

        var code = template
            .Replace("{Usings}", string.Join("\n", usings))
            .Replace("{Namespace}", namespaceName)
            .Replace("{ClassDeclaration}", classDeclaration)
            .Replace("{OriginalClassName}", classToClone.Name)
            .Replace("{MethodName}", methodName)
            .Replace("{ConstructorArgs}", constructorArgs)
            .Replace("{Properties}", string.Join("\n", properties).TrimEnd(','));

        return code;

        static string GetPropertyName(string parameterName, IPropertySymbol[] properties) {
            var property = properties.FirstOrDefault(p => string.Equals(p.Name, parameterName, StringComparison.OrdinalIgnoreCase));
            return property?.Name ?? parameterName;
        }
    }
}