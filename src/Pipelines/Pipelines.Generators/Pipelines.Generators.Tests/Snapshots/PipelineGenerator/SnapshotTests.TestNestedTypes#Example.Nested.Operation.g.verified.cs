//HintName: Example.Nested.Operation.g.cs
using System;
using System.Threading.Tasks;
using Pipelines.Attributes;

#nullable enable

#pragma warning disable CS1998

partial class Example
{
    partial class Nested
    {
        [global::Pipelines.Attributes.PipelineFor(nameof(Operation))]
        static partial class OperationPipeline
        {
            public partial class StepStep : global::Pipelines.IStep<StepStep.Input, StepStep.Output>
            {
                private readonly Example.Nested this_;
                
                public StepStep(Example.Nested @this) =>
                    this_ = @this ?? throw new global::System.ArgumentNullException(nameof(@this));
                
                public async ValueTask<Output> Execute(StepStep.Input in_)
                {
                    Console.WriteLine($"2: {in_.context}");
                    
                    return new Output
                    {
                    };
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
}
