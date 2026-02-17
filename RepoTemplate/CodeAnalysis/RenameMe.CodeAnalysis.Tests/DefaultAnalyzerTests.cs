using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

namespace RenameMe.CodeAnalysis.Tests;

/// <summary>
/// Tests for the <see cref="DefaultAnalyzer"/> to ensure it correctly identifies when the assembly still has a default name and reports the appropriate diagnostics.
/// </summary>
public class DefaultAnalyzerTests
{
    /// <summary>
    /// Tests that a diagnostic is reported when the assembly has default name. The test sets up a simple C# code snippet and modifies the project to have the default assembly name. It then verifies that the expected diagnostic is reported with the correct message.
    /// </summary>
    [Fact]
    public async Task AssemblyWithDefaultName_ReportsDiagnostic()
    {
        var test = new CSharpAnalyzerTest<DefaultAnalyzer, DefaultVerifier>
        {
            TestCode = "class C { }",
        };

        test.SolutionTransforms.Add((solution, projectId) =>
        {
            var project = solution.GetProject(projectId)!;
            return project.WithAssemblyName(DefaultAnalyzer.TemplateDefaultName).Solution;
        });

        test.ExpectedDiagnostics.Add(
            new DiagnosticResult(DefaultAnalyzer.DiagnosticId, DiagnosticSeverity.Warning)
                .WithMessage($"Assembly '{DefaultAnalyzer.TemplateDefaultName}' should be renamed from the template default '{DefaultAnalyzer.TemplateDefaultName}'"));

        await test.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that no diagnostic is reported when the assembly does not have a default name
    /// </summary>
    [Fact]
    public async Task AssemblyWithoutDefaultName_NoDiagnostic()
    {
        var test = new CSharpAnalyzerTest<DefaultAnalyzer, DefaultVerifier>
        {
            TestCode = "class C { }",
        };

        test.SolutionTransforms.Add((solution, projectId) =>
        {
            var project = solution.GetProject(projectId)!;
            return project.WithAssemblyName(DefaultAnalyzer.TemplateDefaultName + "MyProject").Solution;
        });

        await test.RunAsync(TestContext.Current.CancellationToken);
    }
}
