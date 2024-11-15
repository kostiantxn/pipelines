using Microsoft.CodeAnalysis;

namespace Pipelines.Generators;

// TODO: Move to analysers.
// TODO: Prohibit top-level `using var` and `await using var`.
// TODO: Prohibit non-static local methods (for now)? Can allow properly captured variables later.
public static class Diagnostics
{
    public static readonly DiagnosticDescriptor ExpressionBodiedMethodsAreNotSupported =
        new("PI0001",
            "Expression bodied methods are not supported",
            "Expression bodied methods are not supported",
            "Pipelines",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor LocalMethodsAreNotSupported =
        new("PI0002",
            "Nested methods are not supported",
            "Nested methods are not supported",
            "Pipelines",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor PipelineMustStartWithLabel =
        new("PI0003",
            "Pipeline method must start with a label",
            "Pipeline method must start with a label",
            "Pipelines",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor GotoStatementsAreNotAllowed =
        new("PI0004",
            "Goto statements are not allowed in pipeline methods",
            "Goto statements are not allowed in pipeline methods",
            "Pipelines",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor OnlyLastStepCanReturn =
        new("PI0005",
            "Only the last step can contain a return statement",
            "Only the last step can contain a return statement",
            "Pipelines",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);
}