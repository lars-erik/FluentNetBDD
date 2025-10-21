using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using FluentNetBDD.Generation;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace FluentNetBDD.Generators
{
    [Generator]
    public class DslGenerator : IIncrementalGenerator
    {
        private readonly bool enableBuilder;

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

        public DslGenerator()
        {
            
        }

        public DslGenerator(bool enableBuilder)
        {
            this.enableBuilder = enableBuilder;
        }

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            //#if DEBUG
            //            if (!Debugger.IsAttached) Debugger.Launch();
            //#endif

            var classNames = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: IsGenerateDslAttribute,
                    transform: TransformGenerateDslAttributeToDslSet)
                .Where(n => n != null)
                .Select((n, _) => n)
                .Collect();

            context.RegisterSourceOutput(classNames, RegisterSourceOutputs);

        }

        private static bool IsGenerateDslAttribute(SyntaxNode n, CancellationToken cancellationToken)
        {
            return n is AttributeSyntax s &&
                   s.Name is IdentifierNameSyntax i &&
                   nameof(GenerateDslAttribute).Contains(i.Identifier.Text);
        }

        private static DslSet TransformGenerateDslAttributeToDslSet(GeneratorSyntaxContext ctx, CancellationToken cancellationToken)
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
                .Select((x, i) => new { x, i })
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
        }

        private void RegisterSourceOutputs(SourceProductionContext spc, ImmutableArray<DslSet> markers)
        {
            var generatedDslSets = new HashSet<string>();

            foreach (var dslSet in markers)
            {
                var featureName = dslSet.FeatureName;
                if (!generatedDslSets.Add(featureName)) continue;

                var dslClassName = featureName + (featureName.EndsWith("Dsl") ? "" : "Dsl");
                var source = GenerateInnerDsl(featureName, dslClassName, dslSet);
                spc.AddSource($"{dslClassName}.g.cs", source);
            }
        }

        private string GenerateInnerDsl(string featureName, string dslClassName, DslSet dslSet)
        {
            var doubleNewLine = Environment.NewLine + Environment.NewLine;

            var givenTypes = AsTypeInfos(dslSet.Types[0]);
            var whenTypes = AsTypeInfos(dslSet.Types[1]);
            var thenTypes = AsTypeInfos(dslSet.Types[2]);

            var namespaces = givenTypes.Union(whenTypes).Union(thenTypes)
                .Select(x => x.Type!.ContainingNamespace.ToString())
                .Distinct();

            var givenActors = FindSubjunctionActors(givenTypes);
            var whenActors = FindSubjunctionActors(whenTypes);
            var thenActors = FindSubjunctionActors(thenTypes);

            var givenDrivers = CreateSubjunctionDrivers(featureName, "Given", givenActors);
            var whenDrivers = CreateSubjunctionDrivers(featureName, "When", whenActors);
            var thenDrivers = CreateSubjunctionDrivers(featureName, "Then", thenActors);

            var drivers = String.Join(doubleNewLine, [givenDrivers, whenDrivers, thenDrivers]);

            var givenBuilders = CreateSubjunctionBuilders(featureName, "Given", givenActors);
            var whenBuilders = CreateSubjunctionBuilders(featureName, "When", whenActors);
            var thenBuilders = CreateSubjunctionBuilders(featureName, "Then", thenActors);

            var builders = 
                enableBuilder
                    ? String.Join(
                          doubleNewLine, 
                          [givenBuilders, whenBuilders, thenBuilders]
                      )
                    : "";

            var builderDslClass =
                $$"""
                  public partial class {{dslClassName}} : Dsl<{{featureName}}GivenBuilder, {{featureName}}WhenBuilder, {{featureName}}ThenBuilder>
                  {
                      public {{dslClassName}}(IServiceProvider provider)
                      {
                          Given = new {{featureName}}GivenBuilder(provider);
                          When = new {{featureName}}WhenBuilder(provider);
                          Then = new {{featureName}}ThenBuilder(provider);
                      }
                  }
                  """;

            var driverDslClass =
                $$"""
                  public partial class {{dslClassName}} : Dsl<I{{featureName}}GivenDriver, I{{featureName}}WhenDriver, I{{featureName}}ThenDriver>
                  {
                      public {{dslClassName}}(IServiceProvider provider) : base(provider)
                      {
                      }
                  }
                  """;

            var dslClass = enableBuilder ? builderDslClass : driverDslClass;

            var source =
                $$"""
                  using System;
                  using FluentNetBDD.Dsl;
                  {{String.Join(Environment.NewLine, namespaces.Select(ns => $"using {ns};"))}}

                  {{(dslSet.Namespace != "" ? $"namespace {dslSet.Namespace}.Generated;" : "")}}

                  // <auto-generated/>
                  {{drivers}}

                  {{builders}}
                  
                  {{dslClass}}
                  """;
            return source;
        }

        private TypeInfo[] AsTypeInfos(object marker)
        {
            var types = (TypeInfo[])marker;
            return types;
        }

        private string CreateSubjunctionDrivers(string featureName, string subjunctionName, Dictionary<string, TypeInfo[]> subjunctionActors)
        {
            var subjunctionMembers = subjunctionActors
                .ToDictionary(
                    actor => actor.Key,
                    actor =>
                        $$"""
                          public interface I{{featureName}}{{subjunctionName}}{{actor.Key}}Driver : {{String.Join(", ", actor.Value.Select(x => x.Type!.Name))}} {}
                          """
                );

            var subjunctionInterface =
                $$"""
                  public interface I{{featureName}}{{subjunctionName}}Driver
                  {
                  {{String.Join(Environment.NewLine, subjunctionActors.Keys.Select(actor => $"    I{featureName}{subjunctionName}{actor}Driver {actor} {{ get; }}"))}}
                  }
                  """;

            var subjunctionDeclarations =
                $"""
                 {String.Join(Environment.NewLine, subjunctionMembers.Values)}
                 {subjunctionInterface}
                 """;
            return subjunctionDeclarations;
        }

        private string CreateSubjunctionBuilders(string featureName, string subjunctionName, Dictionary<string, TypeInfo[]> subjunctionActors)
        {
            var subjunctionMemberBuilders = subjunctionActors
                .ToDictionary(
                    actor => actor.Key,
                    actor =>
                    {
                        var mainName = String.Concat([featureName, subjunctionName, actor.Key]);
                        return $$"""
                                 public class {{mainName}}Builder
                                 {
                                     private readonly I{{mainName}}Driver driver;
                                     
                                     public {{mainName}}Builder(I{{mainName}}Driver driver)
                                     {
                                         this.driver = driver;
                                     }
                                     
                                 {{String.Join(Environment.NewLine,
                                     actor.Value.Select(typeInfo =>
                                     {
                                         var memberImplementations =
                                             String.Join(Environment.NewLine + "    ",
                                             typeInfo.Type?
                                                .GetMembers()
                                                .OfType<IMethodSymbol>()
                                                .Where(member => member.MethodKind == MethodKind.Ordinary)
                                                .Select(member =>
                                                {
                                                    var parameterList = String.Join(", ", member.Parameters.Select(p => $"{p.Type.Name} {p.Name}")); 
                                                    var argumentList = String.Join(", ", member.Parameters.Select(p => p.Name));
                                                    var code =
                                                        $$"""
                                                            public {{mainName}}Builder {{member.Name}}({{parameterList}})
                                                            {
                                                                {{(
                                                                    member.IsAsync
                                                                    ? $"actions.Add(async () => await driver.{member.Name}({argumentList});"
                                                                    : $"actions.Add(async () => await Task.Run(() => driver.{member.Name}({argumentList});"
                                                                )}}
                                                                return this;
                                                            }
                                                        
                                                        """;
                                                    return code;
                                                })
                                             ?? []
                                             ); ;
                                         return memberImplementations;
                                     })
                                 )}}
                                 }
                                 
                                 """;
                    });

            var subjunctionBuilder =
                $$"""
                  public class {{featureName}}{{subjunctionName}}Builder
                  {
                  {{String.Join(Environment.NewLine, subjunctionActors.Keys.Select(actor => $"    {featureName}{subjunctionName}{actor}Builder {actor} {{ get; }}"))}}
                  }
                  """;

            var subjunctionDeclarations =
                $"""
                 {String.Join(Environment.NewLine, subjunctionMemberBuilders.Values)}
                 {subjunctionBuilder}
                 """;
            return subjunctionDeclarations;
        }

        private static Dictionary<string, TypeInfo[]> FindSubjunctionActors(TypeInfo[] subjunctionTypes)
        {
            // TODO: Validate a bit more non-nullability
            return subjunctionTypes
                .Where(x => x.Type?.GetAttributes().Any(a => a.AttributeClass?.Name == nameof(ActorAttribute)) == true)
                .GroupBy(x => x.Type!.GetAttributes().First(a => a.AttributeClass!.Name == nameof(ActorAttribute)).ConstructorArguments.First().Value!.ToString())
                .ToDictionary(
                    x => x.Key,
                    x => x.ToArray()
                );
        }
    }
}
