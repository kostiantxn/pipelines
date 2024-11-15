//HintName: Example.Nested.Operation.g.cs
using System;
using System.Threading.Tasks;
using Pipelines.Attributes;

#nullable enable

#pragma warning disable CS1998

public partial class Example
{
    public partial class Nested
    {
        [global::Pipelines.Attributes.PipelineFor(nameof(Operation))]
        static partial class OperationPipeline
        {
            public partial class Step1Step : global::Pipelines.IStep<Step1Step.Input, Step1Step.Output>
            {
                private readonly Example.Nested this_;
                
                public Step1Step(Example.Nested @this) =>
                    this_ = @this ?? throw new global::System.ArgumentNullException(nameof(@this));
                
                public async ValueTask<Output> Execute(Step1Step.Input in_)
                {
                    var id = 451;
                    
                    return new Output
                    {
                        context = in_.context,
                        id = id,
                    };
                }
                
                public struct Input
                {
                    public required object context;
                }
                
                public struct Output
                {
                    public required object context;
                    public required int id;
                }
            }
            
            public partial class Step2Step : global::Pipelines.IStep<Step1Step.Output, Step2Step.Output>
            {
                private readonly Example.Nested this_;
                
                public Step2Step(Example.Nested @this) =>
                    this_ = @this ?? throw new global::System.ArgumentNullException(nameof(@this));
                
                public async ValueTask<Output> Execute(Step1Step.Output in_)
                {
                    Console.WriteLine($"{in_.id}: {in_.context}");
                    
                    return new Output
                    {
                    };
                }
                
                public struct Output
                {
                }
            }
        }
    }
}
