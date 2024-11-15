using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Pipelines.Generators.Visitors;

internal class ThisOperationVisitor : OperationVisitor
{
    public readonly List<IInstanceReferenceOperation> References = new();

    public override void DefaultVisit(IOperation operation)
    {
        foreach (var child in operation.ChildOperations)
            Visit(child);
    }

    public override void VisitInstanceReference(IInstanceReferenceOperation operation)
    {
        References.Add(operation);

        base.VisitInstanceReference(operation);
    }
}