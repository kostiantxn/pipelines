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
        public partial class CreateTransferStep : global::Pipelines.IStep<CreateTransferStep.Input, CreateTransferStep.Output>
        {
            private readonly Example this_;
            
            public CreateTransferStep(Example @this) =>
                this_ = @this ?? throw new global::System.ArgumentNullException(nameof(@this));
            
            public async ValueTask<Output> Execute(CreateTransferStep.Input in_)
            {
                var id = 451;
                var name = "Buzz";
                
                Console.WriteLine(
                    $"Created a record for the transfer (ID: {id}, context: {in_.context}).");
                
                return new Output
                {
                    context = in_.context,
                    id = id,
                    name = name,
                };
            }
            
            public record struct Input
            {
                public required object context;
            }
            
            public record struct Output
            {
                public required object context;
                public required int id;
                public required string name;
            }
        }
        
        public partial class SendTransferStep : global::Pipelines.IStep<CreateTransferStep.Output, SendTransferStep.Output>
        {
            private readonly Example this_;
            
            public SendTransferStep(Example @this) =>
                this_ = @this ?? throw new global::System.ArgumentNullException(nameof(@this));
            
            public async ValueTask<Output> Execute(CreateTransferStep.Output in_)
            {
                Console.WriteLine(
                    $"Sent the transfer (ID: {in_.id}).");
                
                return new Output
                {
                    context = in_.context,
                    id = in_.id,
                    name = in_.name,
                };
            }
            
            public record struct Output
            {
                public required object context;
                public required int id;
                public required string name;
            }
        }
        
        public partial class UpdateTransferStep : global::Pipelines.IStep<SendTransferStep.Output, UpdateTransferStep.Output>
        {
            private readonly Example this_;
            
            public UpdateTransferStep(Example @this) =>
                this_ = @this ?? throw new global::System.ArgumentNullException(nameof(@this));
            
            public async ValueTask<Output> Execute(SendTransferStep.Output in_)
            {
                _ = in_.id;
                _ = in_.name;
                
                Console.WriteLine(
                    $"Updated the transfer record (ID: {in_.id}, context: {in_.context}).");
                
                return new Output();
            }
            
            public record struct Output
            {
            }
        }
    }
}
