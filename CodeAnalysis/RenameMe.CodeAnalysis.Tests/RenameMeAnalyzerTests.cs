using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

namespace RenameMe.CodeAnalysis.Tests;

public class RenameMeAnalyzerTests
{
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
            return project.WithAssemblyName("RenameMe").Solution;
        });

        test.ExpectedDiagnostics.Add(
            new DiagnosticResult(RenameMeAnalyzer.DiagnosticId, DiagnosticSeverity.Warning)
                .WithMessage("Assembly 'RenameMe' should be renamed from the template default 'RenameMe'"));

        await test.RunAsync(TestContext.Current.CancellationToken);
    }

    [Theory]
    [InlineData("RENAMEME")]
    [InlineData("renameme")]
    [InlineData("Renameme")]
    public async Task AssemblyNamedRenameMe_CaseInsensitive_ReportsDiagnostic(string assemblyName)
    {
        var test = new CSharpAnalyzerTest<RenameMeAnalyzer, DefaultVerifier>
        {
            TestCode = "class C { }",
        };

        test.SolutionTransforms.Add((solution, projectId) =>
        {
            var project = solution.GetProject(projectId)!;
            return project.WithAssemblyName(assemblyName).Solution;
        });

        test.ExpectedDiagnostics.Add(
            new DiagnosticResult(RenameMeAnalyzer.DiagnosticId, DiagnosticSeverity.Warning)
                .WithMessage($"Assembly '{assemblyName}' should be renamed from the template default 'RenameMe'"));

        await test.RunAsync(TestContext.Current.CancellationToken);
    }

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
            return project.WithAssemblyName("MyProject").Solution;
        });

        await test.RunAsync(TestContext.Current.CancellationToken);
    }
}
