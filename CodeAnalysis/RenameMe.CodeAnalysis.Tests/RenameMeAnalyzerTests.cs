using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

namespace RenameMe.CodeAnalysis.Tests;

/// <summary>
/// Tests for the <see cref="RenameMeAnalyzer"/> to ensure it correctly identifies when the assembly is still named "RenameMe" and reports the appropriate diagnostics.
/// </summary>
public class RenameMeAnalyzerTests
{
    /// <summary>
    /// Tests that a diagnostic is reported when the assembly is named "RenameMe". The test sets up a simple C# code snippet and modifies the project to have the assembly name "RenameMe". It then verifies that the expected diagnostic is reported with the correct message.
    /// </summary>
    [Fact]
    public async Task AssemblyNamedRenameMe_ReportsDiagnostic()
    {
        var test = new CSharpAnalyzerTest<RenameMeAnalyzer, DefaultVerifier>
        {
            TestCode = "class C { }",
        };

        test.SolutionTransforms.Add((solution, projectId) =>
        {
            var project = solution.GetProject(projectId)!;
            return project.WithAssemblyName(RenameMeAnalyzer.TemplateDefaultName).Solution;
        });

        test.ExpectedDiagnostics.Add(
            new DiagnosticResult(RenameMeAnalyzer.DiagnosticId, DiagnosticSeverity.Warning)
                .WithMessage($"Assembly '{RenameMeAnalyzer.TemplateDefaultName}' should be renamed from the template default '{RenameMeAnalyzer.TemplateDefaultName}'"));

        await test.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that no diagnostic is reported when the assembly is not named "RenameMe". The
    /// </summary>
    [Fact]
    public async Task AssemblyNotNamedRenameMe_NoDiagnostic()
    {
        var test = new CSharpAnalyzerTest<RenameMeAnalyzer, DefaultVerifier>
        {
            TestCode = "class C { }",
        };

        test.SolutionTransforms.Add((solution, projectId) =>
        {
            var project = solution.GetProject(projectId)!;
            return project.WithAssemblyName(RenameMeAnalyzer.TemplateDefaultName + "MyProject").Solution;
        });

        await test.RunAsync(TestContext.Current.CancellationToken);
    }
}
