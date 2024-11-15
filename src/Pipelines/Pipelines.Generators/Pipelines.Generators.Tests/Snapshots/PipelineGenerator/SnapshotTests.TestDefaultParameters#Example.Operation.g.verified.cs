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
                    a = in_.a,
                    b = in_.b,
                    c = in_.c,
                };
            }
            
            public record struct Input
            {
                public required int a;
                public required double b;
                public decimal c = 451;
                public string d = "xyz";
            }
            
            public record struct Output
            {
                public required int a;
                public required double b;
                public required decimal c;
            }
        }
        
        public partial class Step2Step : global::Pipelines.IStep<Step1Step.Output, Step2Step.Output>
        {
            public async ValueTask<Output> Execute(Step1Step.Output in_)
            {
                int id;
                
                id = context.GetHashCode();
                
                return new Output
                {
                    a = in_.a,
                    b = in_.b,
                    c = in_.c,
                    id = id,
                };
            }
            
            public record struct Output
            {
                public required int a;
                public required double b;
                public required decimal c;
                public required int id;
            }
        }
        
        public partial class Step3Step : global::Pipelines.IStep<Step2Step.Output, Step3Step.Output>
        {
            public async ValueTask<Output> Execute(Step2Step.Output in_)
            {
                Console.WriteLine(in_.id);
                Console.WriteLine(in_.a + in_.b + in_.c);
                
                return new Output();
            }
            
            public record struct Output
            {
            }
        }
    }
}
