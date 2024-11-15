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
        public partial class Step1Step : global::Pipelines.IStep<Step1Step.Input, Step1Step.Output>
        {
            private readonly Example this_;
            
            public Step1Step(Example @this) =>
                this_ = @this ?? throw new global::System.ArgumentNullException(nameof(@this));
            
            public async ValueTask<Output> Execute(Step1Step.Input in_)
            {
                Console.WriteLine("Hello");
                
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
        
        public partial class Step2Step : global::Pipelines.IStep<Step1Step.Output, Step2Step.Output>
        {
            private readonly Example this_;
            
            public Step2Step(Example @this) =>
                this_ = @this ?? throw new global::System.ArgumentNullException(nameof(@this));
            
            public async ValueTask<Output> Execute(Step1Step.Output in_)
            {
                Console.WriteLine("World");
                
                return new Output
                {
                };
            }
            
            public record struct Output
            {
            }
        }
    }
}
