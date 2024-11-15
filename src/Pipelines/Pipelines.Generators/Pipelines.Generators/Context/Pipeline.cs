using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Pipelines.Generators.Context;

/// <summary>
///     A pipeline description.
/// </summary>
public class Pipeline
{
    /// <summary>
    ///     The method marked with <c>[Pipeline]</c> that defines the pipeline.
    /// </summary>
    public IMethodSymbol Method { get; }

    /// <summary>
    ///     The body of the pipeline method.
    /// </summary>
    public IBlockOperation Body { get; }

    /// <summary>
    ///     The list of steps that the pipeline consists of.
    /// </summary>
    public List<Step> Steps { get; }

    public Pipeline(IMethodSymbol method, IBlockOperation body, SemanticModel model)
    {
        Method = method;
        Body = body;
        Steps = new();

        var index = 0;

        foreach (var operation in body.Operations)
        {
            if (operation is ILabeledOperation labeled)
                Steps.Add(new Step(index++, labeled) { Inflow = null!, Outflow = null! });
            else
                Steps.Last().Operations.Add(operation);
        }

        var last = Steps.Last().Operations.Last();

        foreach (var step in Steps)
        {
            // Calculate dataflow into this step. We need to consider the variables that are read
            // or written inside the step, but which are also written outside it. In this case,
            // the variable must be passed in the output of the previous step.
            step.Inflow = model.AnalyzeDataFlow(
                step.Operations.First().Syntax,
                step.Operations.Last().Syntax);

            // Calculate dataflow into the next steps. We need to also consider the previous steps
            // as well in case certain variables need to be passed through the current step
            var next = Next(step);
            if (next is not null)
                step.Outflow = model.AnalyzeDataFlow(
                    next.Operations.First().Syntax,
                    last.Syntax);
        }
    }

    /// <summary>
    ///     Gets the previous step of the current step.
    /// </summary>
    /// <param name="step">The step to get the previous step of.</param>
    /// <returns>The previous step, or <c>null</c> if the current step is the first one.</returns>
    public Step? Prev(in Step step) =>
        step.Index > 0 ? Steps[step.Index - 1] : null;

    /// <summary>
    ///     Gets the next step of the current step.
    /// </summary>
    /// <param name="step">The step to get the next step of.</param>
    /// <returns>The next step, or <c>null</c> if the current step is the last one.</returns>
    public Step? Next(in Step step) =>
        step.Index + 1 < Steps.Count ? Steps[step.Index + 1] : null;
}
