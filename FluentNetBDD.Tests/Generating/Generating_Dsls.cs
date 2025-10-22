using System.Text;
using FluentNetBDD.Generation;
using FluentNetBDD.Generators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace FluentNetBDD.Tests.Generating;

public class Generating_Dsls
{
    private const string GenerateAgileNamedUserDslSource =
        """
        using FluentNetBDD.Dsl;
        using FluentNetBDD.Generation;
        using FluentNetBDD.Tests.Dsl.UserFeatures;
        
        namespace FluentNetBDD.Tests.Generating;
        
        [GenerateDsl(
            "AgileNamedUser",
            givenTypes: [typeof(IUserWithName), typeof(IUserWithAgility)],
            whenTypes: [typeof(IUserGreetingAction), typeof(IUserAgilityActions)],
            thenTypes: [typeof(IUserGreetingVerification), typeof(IUserAgilityVerification)]
        )]
        
        public class JustSomethingToHangOnTo 
        {
        }
        """;

    [Test]
    public async Task Creates_Builders_For_Actors_And_Subjunctions()
    {
        await GenerateAndVerify(true);
    }

    [Test]
    public async Task Creates_Drivers_For_Actors_And_Subjunctions()
    {
        await GenerateAndVerify(false);
    }

    private static async Task GenerateAndVerify(bool enableBuilder)
    {
        var generatedSource = Generate(GenerateAgileNamedUserDslSource, enableBuilder);
        var bytes = new byte[] { 0xEF, 0xBB, 0xBF }
            .Concat(Encoding.UTF8.GetBytes(generatedSource))
            .ToArray();
        await Verify(bytes, extension: "g.cs")
            .UseDirectory("Approvals");
    }

    private static string Generate(string source, bool enableBuilder)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);

        var references = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => !a.IsDynamic && !string.IsNullOrWhiteSpace(a.Location))
            .Select(a => MetadataReference.CreateFromFile(a.Location))
            .ToList();

        var attributeAssemblyPath = typeof(GenerateDslAttribute).Assembly.Location;
        references.Add(MetadataReference.CreateFromFile(attributeAssemblyPath));

        var compilation = CSharpCompilation.Create(
            "TestAssembly",
            [syntaxTree],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );

        var generator = new DslGenerator(enableBuilder);

        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator)
            .RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);

        var generatedTrees = outputCompilation.SyntaxTrees.ToList();
        var dslSyntaxTree = generatedTrees.Last();
        var generatedSource = dslSyntaxTree.ToString();
        return generatedSource;
    }
}
