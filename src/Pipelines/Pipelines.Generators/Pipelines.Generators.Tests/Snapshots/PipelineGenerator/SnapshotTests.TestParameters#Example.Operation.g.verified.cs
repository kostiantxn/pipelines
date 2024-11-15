//HintName: Example.Operation.g.cs
using System;
using System.Threading.Tasks;
using Pipelines.Attributes;

#nullable enable

#pragma warning disable CS1998

partial class Example
{
    [global::Pipelines.Attributes.PipelineFor(nameof(Operation))]
    static partial class OperationPipeline
    {
        public partial class StepStep : global::Pipelines.IStep<StepStep.Input, StepStep.Output>
        {
            private readonly Example this_;
            
            public StepStep(Example @this) =>
                this_ = @this ?? throw new global::System.ArgumentNullException(nameof(@this));
            
            public async ValueTask<Output> Execute(StepStep.Input in_)
            {
                var hash = in_.context.GetHashCode();
                
                Console.WriteLine(
                    $"The hash code is {hash}.");
                
                return new Output();
            }
            
            public record struct Input
            {
                public required object context;
            }
            
            public record struct Output
            {
            }
        }
    }
}
