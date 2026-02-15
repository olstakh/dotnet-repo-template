using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace RenameMe.CodeAnalysis;

/// <summary>
/// Analyzer that checks if the assembly is still named "RenameMe" and reports a warning if it is.
/// </summary>
/// <remarks>
/// This is a simple example of a Roslyn analyzer that checks for a specific assembly name.
/// Used as a template for creating custom analyzers in the .NET repository.
/// The analyzer will report a warning if the assembly name is still "RenameMe", which is the default name when creating a new project from the template. 
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class RenameMeAnalyzer : DiagnosticAnalyzer
{
    internal const string DiagnosticId = "RENAME001";

    private static readonly DiagnosticDescriptor Rule = new(
        id: DiagnosticId,
        title: "Assembly is still named 'RenameMe'",
        messageFormat: "Assembly '{0}' should be renamed from the template default 'RenameMe'",
        category: "Naming",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Assemblies created from the template should be renamed from the default 'RenameMe' name.",
        customTags: WellKnownDiagnosticTags.CompilationEnd);

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterCompilationAction(AnalyzeCompilation);
    }

    private static void AnalyzeCompilation(CompilationAnalysisContext context)
    {
        var assemblyName = context.Compilation.AssemblyName;

        if (string.Equals(assemblyName, "RenameMe", System.StringComparison.OrdinalIgnoreCase))
        {
            var diagnostic = Diagnostic.Create(Rule, Location.None, assemblyName);
            context.ReportDiagnostic(diagnostic);
        }
    }
}
