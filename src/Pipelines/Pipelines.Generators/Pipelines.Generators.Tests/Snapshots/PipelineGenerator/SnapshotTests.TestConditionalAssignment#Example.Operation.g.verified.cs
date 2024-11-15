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
                var id = 451;
                
                Console.WriteLine(id);
                
                return new Output
                {
                    id = id,
                };
            }
            
            public record struct Input
            {
                public required object context;
            }
            
            public record struct Output
            {
                public required int id;
            }
        }
        
        public partial class Step2Step : global::Pipelines.IStep<Step1Step.Output, Step2Step.Output>
        {
            private readonly Example this_;
            
            public Step2Step(Example @this) =>
                this_ = @this ?? throw new global::System.ArgumentNullException(nameof(@this));
            
            public async ValueTask<Output> Execute(Step1Step.Output in_)
            {
                if (in_.id != 451)
                    in_.id += 1;
                
                Console.WriteLine(in_.id);
                
                return new Output
                {
                    id = in_.id,
                };
            }
            
            public record struct Output
            {
                public required int id;
            }
        }
        
        public partial class Step3Step : global::Pipelines.IStep<Step2Step.Output, Step3Step.Output>
        {
            private readonly Example this_;
            
            public Step3Step(Example @this) =>
                this_ = @this ?? throw new global::System.ArgumentNullException(nameof(@this));
            
            public async ValueTask<Output> Execute(Step2Step.Output in_)
            {
                _ = in_.id;
                
                Console.WriteLine(in_.id);
                
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
