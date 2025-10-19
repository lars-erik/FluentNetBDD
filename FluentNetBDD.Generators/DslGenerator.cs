using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FluentNetBDD.Generation;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace FluentNetBDD.Generators
{
    [Generator]
    internal class DslGenerator : IIncrementalGenerator
    {
        private record DslSet
        {
            public string FeatureName { get; }
            public string Namespace { get; }
            public Dictionary<int, TypeInfo[]> Types { get; }

            public DslSet(string featureName, string ns, Dictionary<int, TypeInfo[]> types)
            {
                FeatureName = featureName?.Trim('"') ?? throw new ArgumentNullException(nameof(featureName));
                Namespace = ns;
                Types = types;
            }

            public void Deconstruct(out string FeatureName, out string Namespace, out Dictionary<int, TypeInfo[]> Types)
            {
                FeatureName = this.FeatureName;
                Namespace = this.Namespace;
                Types = this.Types;
            }
        }

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
//#if DEBUG
//            if (!Debugger.IsAttached) Debugger.Launch();
//#endif

            var classNames = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: (n, _) => n is AttributeSyntax s &&
                                         s.Name is IdentifierNameSyntax i &&
                                         nameof(GenerateDslAttribute).Contains(i.Identifier.Text),
                    transform: (ctx, _) =>
                    {
                        var attr = (AttributeSyntax)ctx.Node;
                        var symbol = ctx.SemanticModel.GetSymbolInfo(attr).Symbol as IMethodSymbol;

                        var curParent = attr.Parent;
                        while (curParent is not null and not BaseNamespaceDeclarationSyntax)
                        {
                            curParent = curParent.Parent;
                        }

                        var ns = "";
                        if (curParent is BaseNamespaceDeclarationSyntax nsDeclaration)
                        {
                            ns = nsDeclaration.Name.ToString();
                        }

                        // TODO: Can we get those by name?
                        var argumentList = 
                            attr.ArgumentList != null &&
                            attr.ArgumentList.Arguments.Any() &&
                            symbol != null &&
                            symbol.ContainingType.Name == nameof(GenerateDslAttribute)
                            ? attr.ArgumentList.Arguments.ToArray()
                            : null;

                        if (argumentList == null) return null;

                        var typeSet = argumentList
                            .Skip(1)
                            .Select((x, i) => new {x, i})
                            .ToDictionary(
                                (a) => a.i,
                                (a) =>
                                {
                                    return ((CollectionExpressionSyntax)a.x.Expression)
                                        .Elements
                                        .OfType<ExpressionElementSyntax>()
                                        .Select(y => y.Expression)
                                        .OfType<TypeOfExpressionSyntax>()
                                        .Select(y => ctx.SemanticModel.GetTypeInfo(y.Type))
                                        .ToArray();
                                });

                        var dslSet = new DslSet(argumentList[0].ToString(), ns, typeSet);

                        return dslSet;

                        /*
                        LEFTOVER EXPERIMENTS FOR REFERENCE

                           var typeLists = argumentList.Skip(1)
                               .Select((x, i) => new {index = i, expr = (CollectionExpressionSyntax)x.Expression})
                               .ToDictionary(
                                   x => x.index,
                                   x => x.expr.Elements
                                   .OfType<ExpressionElementSyntax>()
                                   .Select(y => y.Expression)
                                   .OfType<TypeOfExpressionSyntax>()
                                   .Select(y => ctx.SemanticModel.GetTypeInfo(y.Type))
                                   .ToArray()
                               )
                               ;
                           // We have <int, TypeInfo[]> where TypeInfo >
                           //  Type: ITypeSymbol >
                           //  ISymbol.ContainingNamespace: INamespaceSymbol >
                           //  INamespaceSymbol.UnderlyingNamespaceOrTypeSymbol: NamespaceOrTypeSymbol [SourceNamespaceSymbol] >
                           //  QualifiedName: string

                           var namespaces = argumentList.Skip(1)
                               .Select(x => x.Expression)
                               .OfType<CollectionExpressionSyntax>()
                               .SelectMany(x => x.Elements)
                               .OfType<ExpressionElementSyntax>()
                               .Select(x => x.Expression)
                               .OfType<TypeOfExpressionSyntax>()
                               .Select(x => ctx.SemanticModel.GetTypeInfo(x.Type))
                               .Select(x => String.Join(".", x.Type!.ContainingNamespace.ToString()))
                               .Distinct()
                               .ToArray()
                               ;

                         */

                    })
                .Where(n => n != null)
                .Select((n, _) => n)
                .Collect();

            context.RegisterSourceOutput(classNames, (spc, markers) =>
            {
                var generatedDslSets = new HashSet<string>();

                foreach (var dslSet in markers)
                {
                    var featureName = dslSet.FeatureName;

                    if (!generatedDslSets.Add(featureName)) continue;

                    // CollectionExpressionSyntax > Elements > ExpressionElementSyntax > Expression > TypeOfExpressionSyntax > Type
                    var givenTypes = ExtractTypes(dslSet.Types[0]); 
                    var whenTypes = ExtractTypes(dslSet.Types[1]);
                    var thenTypes = ExtractTypes(dslSet.Types[2]);

                    var namespaces = givenTypes.Union(whenTypes).Union(thenTypes)
                        .Select(x => x.Type!.ContainingNamespace.ToString())
                        .Distinct();

                    var subjunctionName = "Given";
                    var subjunctionTypes = givenTypes;

                    // TODO: Validate a bit more non-nullability
                    var subjunctionActors = subjunctionTypes
                        .Where(x => x.Type?.GetAttributes().Any(a => a.AttributeClass?.Name == nameof(ActorAttribute)) == true)
                        .GroupBy(x => x.Type!.GetAttributes().First(a => a.AttributeClass!.Name == nameof(ActorAttribute)).ConstructorArguments.First().Value!.ToString())
                        .ToDictionary(
                            x => x.Key,
                            x => x.ToArray()
                            );

                    var subjunctionMembers = subjunctionActors
                        .ToDictionary(
                            actor => actor.Key,
                            actor => 
                            $$"""
                              public interface I{{featureName}}{{subjunctionName}}{{actor.Key}} : {{String.Join(", ", actor.Value.Select(x => x.Type!.Name))}} {}
                              """
                        );

                    var subjunctionInterface =
                        $$"""
                        public interface I{{featureName}}{{subjunctionName}}
                        {
                        {{String.Join(Environment.NewLine, subjunctionActors.Keys.Select(actor => $"    I{featureName}{subjunctionName}{actor} {actor} {{ get; }}"))}}
                        }
                        """;

                    var subjunctionDeclarations = 
                        $"""
                        {String.Join(Environment.NewLine, subjunctionMembers.Values)}
                        {subjunctionInterface}
                        """;

                    var whenActors = whenTypes
                        .Where(x => x.Type?.GetAttributes().Any(a => a.AttributeClass?.Name == nameof(ActorAttribute)) == true)
                        .GroupBy(x => x.Type!.GetAttributes().First(a => a.AttributeClass!.Name == nameof(ActorAttribute)).ConstructorArguments.First().Value!.ToString())
                        .ToDictionary(
                            x => x.Key,
                            x => x.ToArray()
                            );

                    var whenMembers = whenActors
                        .ToDictionary(
                            actor => actor.Key,
                            actor => 
                            $$"""
                              public interface I{{featureName}}When{{actor.Key}} : {{String.Join(", ", actor.Value.Select(x => x.Type!.Name))}} {}
                              """
                        );

                    var whenInterface =
                        $$"""
                          public interface I{{featureName}}When
                          {
                          {{String.Join(Environment.NewLine, whenActors.Keys.Select(actor => $"    I{featureName}When{actor} {actor} {{ get; }}"))}}
                          }
                          """;

                    var thenActors = thenTypes
                        .Where(x => x.Type?.GetAttributes().Any(a => a.AttributeClass?.Name == nameof(ActorAttribute)) == true)
                        .GroupBy(x => x.Type!.GetAttributes().First(a => a.AttributeClass!.Name == nameof(ActorAttribute)).ConstructorArguments.First().Value!.ToString())
                        .ToDictionary(
                            x => x.Key,
                            x => x.ToArray()
                            );

                    var thenMembers = thenActors
                        .ToDictionary(
                            actor => actor.Key,
                            actor => 
                            $$"""
                              public interface I{{featureName}}Then{{actor.Key}} : {{String.Join(", ", actor.Value.Select(x => x.Type!.Name))}} {}
                              """
                        );

                    var thenInterface =
                        $$"""
                          public interface I{{featureName}}Then
                          {
                          {{String.Join(Environment.NewLine, thenActors.Keys.Select(actor => $"    I{featureName}Then{actor} {actor} {{ get; }}"))}}
                          }
                          """;

                    var dslClassName = featureName + (featureName.EndsWith("Dsl") ? "" : "Dsl");
                    spc.AddSource($"{dslClassName}.g.cs", 
                        $$"""
                        using System;
                        using FluentNetBDD.Dsl;
                        {{String.Join(Environment.NewLine, namespaces.Select(ns => $"using {ns};"))}}
                        
                        {{(dslSet.Namespace != "" ? $"namespace {dslSet.Namespace};" : "")}}
                        
                        // <auto-generated/>
                        {{subjunctionDeclarations}}
                        
                        {{String.Join(Environment.NewLine, whenMembers.Values)}}
                        {{whenInterface}}
                        
                        {{String.Join(Environment.NewLine, thenMembers.Values)}}
                        {{thenInterface}}
                        
                        public partial class {{dslClassName}} : Dsl<I{{featureName}}Given, I{{featureName}}When, I{{featureName}}Then>
                        {
                            public {{dslClassName}}(IServiceProvider provider) : base(provider)
                            {
                            
                            }
                        }
                        """
                    );
                }
            });

        }

        private TypeInfo[] ExtractTypes(object marker)
        {
            var types = (TypeInfo[])marker;
            return types;
        }
    }
}
