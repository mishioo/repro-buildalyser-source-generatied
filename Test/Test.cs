using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Buildalyzer;
using Buildalyzer.Workspaces;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Host;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Example.Tests.SourceGeneratedIds;

[TestClass]
public class Test
{
    private static readonly string exampleDirectoryPath = Path.Combine(
        Path.Combine(Directory.GetParent(typeof(Test).Assembly.Location)!.FullName, "Example")
    );

    [TestMethod]
    public async Task TestUsingBuildalyser()
    {
        // Get the path to the Example.csproj file
        var manager = new AnalyzerManager();

        var analyzer = manager.GetProject(Path.Combine(exampleDirectoryPath, "Example.csproj"));
        var results = analyzer.Build();
        var result = results.Single();

        var workspace = result.GetWorkspace();
        var compilation =
            await workspace.CurrentSolution.Projects.Single().GetCompilationAsync()
            ?? throw new InvalidOperationException("Compilation was not created");

        // Check for compilation errors
        var errors = compilation.GetDiagnostics().Where(d => d.WarningLevel == 0);
        if (errors.Any())
        {
            var errorMessages = string.Join("\n", errors.Select(e => e.ToString()));
            Assert.Fail($"Compilation failed with errors:\n{errorMessages}");
        }
    }

    [TestMethod]
    public async Task TestUsingRoslyn()
    {
        var exampleSourceCode = File.ReadAllText(Path.Combine(exampleDirectoryPath, "Example.cs"));

        var workspace = new AdhocWorkspace();
        var loader = workspace.Services.GetRequiredService<IAnalyzerService>().GetLoader();

        var references = new List<MetadataReference>
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(JsonSerializer).Assembly.Location),
        };
        Assembly
            .GetEntryAssembly()
            ?.GetReferencedAssemblies()
            .ToList()
            .ForEach(a =>
                references.Add(MetadataReference.CreateFromFile(Assembly.Load(a).Location))
            );

        var generatorReference = new AnalyzerFileReference(
            typeof(JsonSerializerContext).Assembly.Location,
            loader
        );

        var projectInfo = ProjectInfo.Create(
            ProjectId.CreateNewId(),
            VersionStamp.Create(),
            "InMemoryProject",
            "InMemoryProject",
            LanguageNames.CSharp,
            compilationOptions: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary),
            metadataReferences: references,
            analyzerReferences: [generatorReference]
        );

        var newProject = workspace.AddProject(projectInfo);
        var document = newProject.AddDocument("Example.cs", exampleSourceCode);
        var project = document.Project;

        var compilation =
            await project.GetCompilationAsync()
            ?? throw new InvalidOperationException("Compilation was not created");

        // Check for compilation errors
        var errors = compilation.GetDiagnostics().Where(d => d.WarningLevel == 0);
        if (errors.Any())
        {
            var errorMessages = string.Join("\n", errors.Select(e => e.ToString()));
            Assert.Fail($"Compilation failed with errors:\n{errorMessages}");
        }
    }
}
