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
            public async ValueTask<Output> Execute(Step1Step.Input in_)
            {
                var id = 451;
                
                return new Output
                {
                    context = in_.context,
                };
            }
            
            public record struct Input
            {
                public required object context;
            }
            
            public record struct Output
            {
                public required object context;
            }
        }
        
        public partial class Step2Step : global::Pipelines.IStep<Step1Step.Output, Step2Step.Output>
        {
            public async ValueTask<Output> Execute(Step1Step.Output in_)
            {
                int id;
                
                id = await Whatever();
                
                return new Output
                {
                    context = in_.context,
                    id = id,
                };
            }
            
            public record struct Output
            {
                public required object context;
                public required int id;
            }
        }
        
        public partial class Step3Step : global::Pipelines.IStep<Step2Step.Output, Step3Step.Output>
        {
            public async ValueTask<Output> Execute(Step2Step.Output in_)
            {
                if (in_.id == 451)
                    in_.id = in_.context.GetHashCode();
                
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
