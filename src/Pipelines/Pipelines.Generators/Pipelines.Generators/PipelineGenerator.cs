using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Pipelines.Generators;

[Generator]
public class PipelineGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var provider = context
            .SyntaxProvider
            .ForAttributeWithMetadataName(
                "Pipelines.Attributes.PipelineAttribute", 
                (x, _) => x is MethodDeclarationSyntax,
                (x, _) => (Node: x.TargetNode, Symbol: x.TargetSymbol, x.SemanticModel));

        context.RegisterSourceOutput(
            provider,
            (cx, source) => Generate(
                cx,
                (MethodDeclarationSyntax) source.Node,
                source.Symbol as IMethodSymbol,
                source.SemanticModel));
    }

    private static void Generate(SourceProductionContext cx, MethodDeclarationSyntax node, IMethodSymbol? method, SemanticModel model)
    {
        // Nothing to do if the symbol is not available.
        if (method is null)
            return;

        // Local methods are not supported.
        if (method.ContainingType is null)
            return;

        // Only async methods are supported (currently).
        if (!method.IsAsync)
            return;

        // Expression-bodied methods are not supported.
        if (node.ExpressionBody is not null)
            return;

        // We can't generate a pipeline for methods without body.
        if (node.Body is null)
            return;

        // We can't generate a pipeline for methods without operations.
        var operation = model.GetOperation(node.Body);
        if (operation is not IBlockOperation body || body.Operations.Length == 0)
            return;

        // Methods must start with a leading label.
        if (body.Operations[0] is not ILabeledOperation)
            return;

        cx.AddSource(Name(method), Templates.Pipeline(cx, method, body, model));
    }

    private static string Name(IMethodSymbol method)
    {
        var builder = new StringBuilder();
        var type = method.ContainingType;

        do
        {
            builder.Insert(0, '.');
            builder.Insert(0, type.Name);
        } while ((type = type.ContainingType) is not null);

        builder.Append(method.Name);
        builder.Append(".g");

        return builder.ToString();
    }
}