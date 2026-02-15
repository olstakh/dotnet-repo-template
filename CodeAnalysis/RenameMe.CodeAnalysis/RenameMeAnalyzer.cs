using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace RenameMe.CodeAnalysis;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class RenameMeAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "RENAME001";

    private static readonly DiagnosticDescriptor Rule = new(
        id: DiagnosticId,
        title: "Assembly is still named 'RenameMe'",
        messageFormat: "Assembly '{0}' should be renamed from the template default 'RenameMe'",
        category: "Naming",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Assemblies created from the template should be renamed from the default 'RenameMe' name.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

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
