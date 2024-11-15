using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using Pipelines.Generators.Context;
using Pipelines.Generators.Text;
using Pipelines.Generators.Text.CSharp;
using Pipelines.Generators.Visitors;

namespace Pipelines.Generators;

internal static class Templates
{
    // TODO: Support generic pipeline definition methods.
    // TODO: Support sync and async methods.
    // TODO: Support early `return`? Or emit an error.
    // TODO: Make sure exceptions are handled properly.
    // TODO: Only generate `async` methods if they have `await`s.
    public static string Pipeline(SourceProductionContext cx, IMethodSymbol method, IBlockOperation body, SemanticModel model)
    {
        var root = model.SyntaxTree.GetRoot(cx.CancellationToken);
        var code = new Code { Newline = Newline(root) ?? "\n" };

        code.Namespace(method.ContainingNamespace);
        code.Usings(root);
        code.Directives();
        code.ContainingType(method.ContainingType, () =>
        {
            code.PipelineType(new(method, body, model), model);
        });

        return code.ToString();
    }

    private static void Namespace(this Code code, INamespaceSymbol @namespace)
    {
        if (!@namespace.IsGlobalNamespace)
            code.Line($"namespace {@namespace.ToDisplayString()};")
                .Line();
    }

    private static void Usings(this Code code, SyntaxNode root)
    {
        var usings = root.DescendantNodesAndSelf().OfType<UsingDirectiveSyntax>();
        var any = false;

        foreach (var @using in usings)
        {
            code.Line(@using.ToString());
            any = true;
        }

        if (any)
            code.Line();
    }

    private static void Directives(this Code code)
    {
        code.Line("#nullable enable")
            .Line()
            .Line("#pragma warning disable CS1998")
            .Line();
    }

    private static void ContainingType(this Code code, INamedTypeSymbol type, Action body)
    {
        var containers = new List<INamedTypeSymbol>(capacity: 1);
        do
        {
            containers.Insert(0, type);
        }
        while ((type = type.ContainingType) is not null);

        foreach (var container in containers)
            code.Line($"partial class {container.Name}")
                .Line("{")
                .Indent();

        body();

        foreach (var _ in containers)
            code.Dedent()
                .Line("}");
    }

    private static void PipelineType(this Code code, Pipeline pipeline, SemanticModel model)
    {
        // We don't want to specify accessibility here in order to allow the user to
        // change it if they want to by explicitly defining a partial class in their code:
        //
        // public partial class SomePipeline;
        code.Line($"[global::Pipelines.Attributes.PipelineFor(nameof({pipeline.Method.Name}))]");
        code.Block($"static partial class {pipeline.Method.Name}Pipeline", () =>
        {
            for (var i = 0; i < pipeline.Steps.Count; ++i)
            {
                code.StepType(pipeline, pipeline.Steps[i], model);

                if (i < pipeline.Steps.Count - 1)
                    code.Line();
            }
        });
    }

    private static void StepType(this Code code, Pipeline pipeline, Step step, SemanticModel model)
    {
        var prev = pipeline.Prev(step);

        var input = prev is null ? step.Name + "Step.Input" : prev.Name + "Step.Output";

        code.Block($"public partial class {step.Name}Step : global::Pipelines.IStep<{input}, {step.Name}Step.Output>", () =>
        {
            // We require `this` to be passed to the step constructor if the method is not static,
            // even if the reference is not used anywhere in the code of this step.
            if (!pipeline.Method.IsStatic)
                code.Line($"private readonly {pipeline.Method.ContainingType.ToDisplayString()} this_;")
                    .Line()
                    .Line($"public {step.Name}Step({pipeline.Method.ContainingType.ToDisplayString()} @this) =>")
                    .Scope(() =>
                    {
                        code.Line("this_ = @this ?? throw new global::System.ArgumentNullException(nameof(@this));");
                    })
                    .Line();

            code.ExecuteMethod(pipeline, step, input, model);
            code.Line();

            if (prev is null)
            {
                code.InputType(pipeline.Method);
                code.Line();
            }

            code.OutputType(pipeline, step);
        });
    }

    private static void InputType(this Code code, IMethodSymbol method)
    {
        code.Block("public record struct Input", () =>
        {
            foreach (var parameter in method.Parameters)
            {
                var type = parameter.Type.ToDisplayString();
                var name = parameter.Name;

                if (parameter.IsOptional && parameter.HasExplicitDefaultValue)
                {
                    var @default = SymbolDisplay.FormatPrimitive(
                        parameter.ExplicitDefaultValue!,
                        quoteStrings: true,
                        useHexadecimalNumbers: false);

                    code.Line($"public {type} {name} = {@default};");
                }
                else
                    code.Line($"public required {type} {name};");
            }
        });
    }

    private static void OutputType(this Code code, Pipeline pipeline, Step step)
    {
        code.Block("public record struct Output", () =>
        {
            var next = pipeline.Next(step);
            if (next is null)
            {
                var type = pipeline.Method.ReturnType as INamedTypeSymbol;
                if (type is null)
                    return;

                if (type.OriginalDefinition.ToDisplayString() is
                    "System.Threading.Tasks.Task<TResult>" or
                    "System.Threading.Tasks.ValueTask<TResult>")
                {
                    var value = type.TypeArguments[0];

                    code.Line($"public required {value.ToDisplayString()} value;")
                        .Line()
                        .Line($"public static implicit operator Output({value} value) => new() {{ value = value }};");
                }
            }

            if (step.Outflow is null)
                return;

            foreach (var variable in step.Outflow.DataFlowsIn)
            {
                var (name, type) = variable switch
                {
                    ILocalSymbol x => (x.Name, x.Type),
                    IParameterSymbol x => (x.Name, x.Type),
                    _ => (null, null),
                };

                if (name is null || type is null)
                    continue;

                code.Line($"public required {type.ToDisplayString()} {name};");
            }
        });
    }

    private static void ExecuteMethod(this Code code, Pipeline pipeline, Step step, string input, SemanticModel model)
    {
        code.Block($"public async global::System.Threading.Tasks.ValueTask<Output> Execute({input} in_)", () =>
        {
            code.DeclareLocalVariables(step);

            foreach (var nested in step.Operations)
            {
                var operation = nested is ILabeledOperation { Operation: not null } labeled
                    ? labeled.Operation
                    : nested;

                // A mapping between the original operation syntax nodes and the updated syntax
                // nodes that should be used in the step `Execute` method.
                var nodes = new Dictionary<SyntaxNode, SyntaxNode>();

                ReplaceThisReferences(operation, nodes);
                ReplaceInputReferences(operation, nodes);

                var syntax = operation.Syntax.ReplaceNodes(nodes.Keys, (x, _) => nodes[x]);
                var lines = syntax.ToFullString().Split(["\r\n", "\r", "\n"], StringSplitOptions.None);
                var text = string.Join(code.Newline, Dedent(lines));

                code.Text(text);
            }

            code.Return(pipeline, step);
        });

        void ReplaceThisReferences(IOperation operation, Dictionary<SyntaxNode, SyntaxNode> nodes)
        {
            // Replace references to `this` (both implicit and explicit) in the pipeline method
            // with references to private `this_` field of the step class.
            var these = new ThisOperationVisitor();

            these.Visit(operation);

            foreach (var reference in these.References)
            {
                if (reference.IsImplicit)
                    nodes.Add(
                        reference.Syntax,
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            SyntaxFactory.IdentifierName("this_"),
                            SyntaxFactory.IdentifierName(reference.Syntax.ToString())).WithTriviaFrom(reference.Syntax));
                else
                    nodes.Add(
                        reference.Syntax,
                        SyntaxFactory.IdentifierName("this_").WithTriviaFrom(reference.Syntax));
            }
        }

        void ReplaceInputReferences(IOperation operation, Dictionary<SyntaxNode, SyntaxNode> nodes)
        {
            // Replace references to variables that flow into the current step, and are thus
            // being passed in the output of the previous step.
            foreach (var variable in step.Inflow.DataFlowsIn)
            {
                var names = operation.Syntax
                    .DescendantNodesAndSelf()
                    .OfType<IdentifierNameSyntax>()
                    .Where(x => SymbolEqualityComparer.Default.Equals(model.GetSymbolInfo(x).Symbol, variable));

                foreach (var name in names)
                    nodes.Add(
                        name,
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            SyntaxFactory.IdentifierName("in_"),
                            SyntaxFactory.IdentifierName(name.Identifier.Text)).WithTriviaFrom(name));
            }
        }
    }

    private static void DeclareLocalVariables(this Code code, Step step)
    {
        // Declare local variables for variables that were declared in previous steps in
        // the original pipeline method, but which were not passed in the output of
        // the previous step (for example, if a variable is declared but is never assigned
        // in the previous step, or if it is immediately reassigned in the current step).
        var declared = false;
        var variables = new HashSet<ISymbol>(SymbolEqualityComparer.Default);

        foreach (var variable in step.Inflow.ReadInside)
            variables.Add(variable);
        foreach (var variable in step.Inflow.WrittenInside)
            variables.Add(variable);

        foreach (var variable in variables)
        {
            var (name, type) = variable switch
            {
                ILocalSymbol x => (x.Name, x.Type),
                IParameterSymbol x => (x.Name, x.Type),
                _ => (null, null),
            };

            if (name is null || type is null)
                continue;

            // We only need to declare a local variable if 1) it's not already declared within
            // the current step, and 2) if values assigned outside the step are not used.
            if (step.Inflow.VariablesDeclared.All(x => x.Name != name) &&
                step.Inflow.DataFlowsIn.All(x => x.Name != name))
            {
                code.Line($"{type.ToDisplayString()} {name};");

                declared = true;
            }
        }

        if (declared)
            code.Line();
    }

    private static void Return(this Code code, Pipeline pipeline, Step step)
    {
        var next = pipeline.Next(step);
        if (next is null)
        {
            var type = pipeline.Method.ReturnType;
            if (type.OriginalDefinition.ToDisplayString() is
                not "System.Threading.Tasks.Task<TResult>" and
                not "System.Threading.Tasks.ValueTask<TResult>")
                code.Line()
                    .Line("return new Output();");
            else
                code.Line();

            return;
        }

        code.Line()
            .Line("return new Output")
            .Line("{")
            .Scope(() =>
            {
                if (step.Outflow is null)
                    return;

                // Return any variables that are passed into the next steps.
                foreach (var variable in step.Outflow.DataFlowsIn)
                {
                    var name = variable switch
                    {
                        ILocalSymbol x => x.Name,
                        IParameterSymbol x => x.Name,
                        _ => null,
                    };

                    // If the variable either flows into the current step, or if it's a pass-through
                    // variable that is never read or written inside the current step, then it needs
                    // to be read from the input `in_`.
                    if (step.Inflow.DataFlowsIn.Any(x => x.Name == name) ||
                        (step.Inflow.ReadInside.All(x => x.Name != name) &&
                         step.Inflow.WrittenInside.All(x => x.Name != name)))
                        code.Line($"{name} = in_.{name},");
                    // Otherwise, the variable is a local one.
                    else
                        code.Line($"{name} = {name},");
                }
            })
            .Line("};");
    }

    private static string? Newline(SyntaxNode root)
    {
        foreach (var node in root.DescendantNodes())
        {
            string? newline;

            if (node.HasLeadingTrivia && (newline = Newline(node.GetLeadingTrivia())) is not null)
                return newline;
            if (node.HasTrailingTrivia && (newline = Newline(node.GetTrailingTrivia())) is not null)
                return newline;
        }

        return null;
    }

    private static string? Newline(SyntaxTriviaList trivia)
    {
        foreach (var node in trivia)
        {
            if (node.Kind() is not SyntaxKind.EndOfLineTrivia)
                continue;

            var text = node.ToString();
            if (text.EndsWith("\r\n"))
                return "\r\n";
            if (text.EndsWith("\r"))
                return "\r";
            if (text.EndsWith("\n"))
                return "\n";
        }

        return null;
    }

    private static IEnumerable<string> Dedent(IReadOnlyCollection<string> lines)
    {
        var indent = lines
            .Select(Indent)
            .Where(x => x is not null)
            .DefaultIfEmpty()
            .Min(x => x ?? 0);

        return lines.Select(x => x.Substring(Math.Min(x.Length, indent)));

        static int? Indent(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
                return null;

            var i = 0;

            while (i < line.Length && line[i] == ' ')
                ++i;

            return i;
        }
    }
}