using System;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Pipelines.Generators.Extensions;

/// <summary>
///     Extensions for reporting diagnostics.
/// </summary>
internal static class DiagnosticsExtensions
{
    public static void Report(this DiagnosticDescriptor self, Action<Diagnostic> report, Location loc, params object[] args)
    {
        report(Diagnostic.Create(self, loc, args.Select(Encode).ToArray()));

        object Encode(object x) =>
            x.ToString().Replace('<', '{').Replace('>', '}');
    }

    public static void Report(this DiagnosticDescriptor self, SourceProductionContext cx, Location loc, params object[] args) =>
        self.Report(cx.ReportDiagnostic, loc, args);

    public static void Report(this DiagnosticDescriptor self, SourceProductionContext cx, SyntaxNode loc, params object[] args) =>
        self.Report(cx, loc.GetLocation(), args);

    public static void Report(this DiagnosticDescriptor self, SourceProductionContext cx, SyntaxToken loc, params object[] args) =>
        self.Report(cx, loc.GetLocation(), args);
}
