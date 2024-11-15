using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Pipelines.Generators.Context;

/// <summary>
///     A pipeline step description.
/// </summary>
public class Step
{
    /// <summary>
    ///     The index of the step in the pipeline.
    /// </summary>
    public int Index { get; }

    /// <summary>
    ///     The name of the step.
    /// </summary>
    /// <remarks>
    ///     Derived from the label name.
    /// </remarks>
    public string Name { get; }

    /// <summary>
    ///     The list of operations that the step consists of.
    /// </summary>
    public List<IOperation> Operations { get; }

    /// <summary>
    ///     Dataflow into the current step.
    /// </summary>
    /// <remarks>
    ///     Used to calculate variables that are passed into the step from the previous steps.
    /// </remarks>
    public DataFlowAnalysis Inflow { get; set; } = null!;

    /// <summary>
    ///     Dataflow out of the current and the previous steps into the next steps.
    /// </summary>
    /// <remarks>
    ///     Used to calculate variables that need to be returned in the output of the step.
    /// </remarks>
    public DataFlowAnalysis? Outflow { get; set; } = null!;

    public Step(int index, ILabeledOperation label)
    {
        Index = index;
        Name = label.Label.Name;
        Operations = [label];
    }
}
