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
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
//#if DEBUG
//            if (!Debugger.IsAttached) Debugger.Launch();
//#endif

            var classNames = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: (n, _) => n is AttributeSyntax s &&
                                         s.Name is IdentifierNameSyntax i &&
                                         nameof(ConsumesDslAttribute).Contains(i.Identifier.Text),
                    transform: (ctx, _) =>
                    {
                        var attr = (AttributeSyntax)ctx.Node;
                        var symbol = ctx.SemanticModel.GetSymbolInfo(attr).Symbol as IMethodSymbol;

                        var argumentList = 
                            attr.ArgumentList != null &&
                            attr.ArgumentList.Arguments.Any() &&
                            symbol != null &&
                            symbol.ContainingType.Name == nameof(ConsumesDslAttribute)
                            ? attr.ArgumentList.Arguments.ToArray()
                            : null;

                        if (argumentList == null) return null;

                        return argumentList
                            .Select((x, i) => new {x, i})
                            .ToDictionary(
                                (a) => a.i,
                                (a) =>
                                {
                                    return a.i == 0
                                        ? (object)a.x.ToString()
                                        : ((CollectionExpressionSyntax)a.x.Expression)
                                        .Elements
                                        .OfType<ExpressionElementSyntax>()
                                        .Select(y => y.Expression)
                                        .OfType<TypeOfExpressionSyntax>()
                                        .Select(y => ctx.SemanticModel.GetTypeInfo(y.Type))
                                        .ToArray();
                                });

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
                var generatedMarkers = new HashSet<string>();

                foreach (var dictionary in markers)
                {
                    var className = dictionary[0].ToString().Trim('"');

                    // CollectionExpressionSyntax > Elements > ExpressionElementSyntax > Expression > TypeOfExpressionSyntax > Type
                    var givenTypes = ExtractTypes(dictionary[1]); 
                    var whenTypes = ExtractTypes(dictionary[2]);
                    var thenTypes = ExtractTypes(dictionary[3]);

                    var namespaces = givenTypes.Union(whenTypes).Union(thenTypes)
                        .Select(x => x.Type!.ContainingNamespace.ToString())
                        .Distinct();

                    // TODO: Validate a bit more non-nullability
                    var givenActors = givenTypes
                        .Where(x => x.Type?.GetAttributes().Any(a => a.AttributeClass?.Name == nameof(ActorAttribute)) == true)
                        .GroupBy(x => x.Type!.GetAttributes().First(a => a.AttributeClass!.Name == nameof(ActorAttribute)).ConstructorArguments.First().Value!.ToString())
                        .ToDictionary(
                            x => x.Key,
                            x => x.ToArray()
                            );

                    var givenMembers = givenActors
                        .ToDictionary(
                            actor => actor.Key,
                            actor => 
                            $$"""
                              public interface I{{className}}Given{{actor.Key}} : {{String.Join(", ", actor.Value.Select(x => x.Type!.Name))}} {}
                              """
                        );

                    var givenInterface =
                        $$"""
                        public interface I{{className}}Given
                        {
                        {{String.Join(Environment.NewLine, givenActors.Keys.Select(actor => $"    I{className}Given{actor} {actor} {{ get; }}"))}}
                        }
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
                              public interface I{{className}}When{{actor.Key}} : {{String.Join(", ", actor.Value.Select(x => x.Type!.Name))}} {}
                              """
                        );

                    var whenInterface =
                        $$"""
                          public interface I{{className}}When
                          {
                          {{String.Join(Environment.NewLine, whenActors.Keys.Select(actor => $"    I{className}When{actor} {actor} {{ get; }}"))}}
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
                              public interface I{{className}}Then{{actor.Key}} : {{String.Join(", ", actor.Value.Select(x => x.Type!.Name))}} {}
                              """
                        );

                    var thenInterface =
                        $$"""
                          public interface I{{className}}Then
                          {
                          {{String.Join(Environment.NewLine, thenActors.Keys.Select(actor => $"    I{className}Then{actor} {actor} {{ get; }}"))}}
                          }
                          """;

                    var givenTypeList = String.Join(", ", givenTypes.Select(x => x.Type!.Name));
                    var whenTypeList = String.Join(", ", whenTypes.Select(x => x.Type!.Name));
                    var thenTypeList = String.Join(", ", thenTypes.Select(x => x.Type!.Name));
                    
                    spc.AddSource($"{className}.g.cs", 
                        $$"""
                        using System;
                        using FluentNetBDD.Dsl;
                        {{String.Join(Environment.NewLine, namespaces.Select(ns => $"using {ns};"))}}
                        
                        // <auto-generated/>
                        {{String.Join(Environment.NewLine, givenMembers.Values)}}
                        {{givenInterface}}
                        
                        {{String.Join(Environment.NewLine, whenMembers.Values)}}
                        {{whenInterface}}
                        
                        {{String.Join(Environment.NewLine, thenMembers.Values)}}
                        {{thenInterface}}
                        
                        public class {{className}} : Dsl<I{{className}}Given, I{{className}}When, I{{className}}Then>
                        {
                            public {{className}}(IServiceProvider provider) : base(provider)
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
