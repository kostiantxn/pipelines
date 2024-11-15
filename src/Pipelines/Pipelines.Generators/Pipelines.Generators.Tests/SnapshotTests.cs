using System.Threading.Tasks;
using Pipelines.Attributes;
using VerifyXunit;
using Xunit;

namespace Pipelines.Generators.Tests;

[UsesVerify]
public class SnapshotTests
{
    [Fact]
    public Task TestHelloWorld() =>
        Verify(
            // lang=C#
            """
            using System;
            using System.Threading.Tasks;
            using Pipelines.Attributes;

            public class Example
            {
                [Pipeline]
                public async Task Operation(object context)
                {
                Step1:
                    Console.WriteLine("Hello");
                    
                Step2:
                    Console.WriteLine("World");
                }
            }
            """);

    [Fact]
    public Task TestVariables() =>
        Verify(
            // lang=C#
            """
            using System;
            using System.Threading.Tasks;
            using Pipelines.Attributes;

            public class Example
            {
                [Pipeline]
                public async Task Operation(object context)
                {
                CreateTransfer:
                    var id = 451;
                    var name = "Buzz";

                    Console.WriteLine(
                        $"Created a record for the transfer (ID: {id}, context: {context}).");

                SendTransfer:
                    Console.WriteLine(
                        $"Sent the transfer (ID: {id}).");

                UpdateTransfer:
                    _ = id;
                    _ = name;

                    Console.WriteLine(
                        $"Updated the transfer record (ID: {id}, context: {context}).");
                }
            }
            """);

    [Fact]
    public Task TestConditionalAssignment() =>
        Verify(
            // lang=C#
            """
            using System;
            using System.Threading.Tasks;
            using Pipelines.Attributes;

            public class Example
            {
                [Pipeline]
                public async Task Operation(object context)
                {
                Step1:
                    var id = 451;

                    Console.WriteLine(id);

                Step2:
                    if (id != 451)
                        id += 1;

                    Console.WriteLine(id);

                Step3:
                    _ = id;

                    Console.WriteLine(id);
                }
            }
            """);

    [Fact]
    public Task TestFields() =>
        Verify(
            // lang=C#
            """
            using System;
            using System.Threading.Tasks;
            using Pipelines.Attributes;

            public partial class Example
            {
                private readonly object _whatever = new();

                [Pipeline]
                public async Task Operation(object context)
                {
                Step:
                    var hash = _whatever.GetHashCode();

                    Console.WriteLine(
                        $"The hash code is {hash}.");
                }
            }
            """);

    [Fact]
    public Task TestParameters() =>
        Verify(
            // lang=C#
            """
            using System;
            using System.Threading.Tasks;
            using Pipelines.Attributes;

            public partial class Example
            {
                [Pipeline]
                public async Task Operation(object context)
                {
                Step:
                    var hash = context.GetHashCode();

                    Console.WriteLine(
                        $"The hash code is {hash}.");
                }
            }
            """);

    [Fact]
    public Task TestMethods() =>
        Verify(
            // lang=C#
            """
            using System;
            using System.Threading.Tasks;
            using Pipelines.Attributes;

            public partial class Example
            {
                [Pipeline]
                public async Task Operation(object context)
                {
                Step:
                    await this.Whatever();
                }

                private Task Whatever() => Task.CompletedTask;
            }
            """);

    [Fact]
    public Task TestStatic() =>
        Verify(
            // lang=C#
            """
            using System;
            using System.Threading.Tasks;
            using Pipelines.Attributes;

            public partial class Example
            {
                [Pipeline]
                public static async Task Operation(object context)
                {
                Step1:
                    var id = 451;

                Step2:
                    id = await Whatever();

                Step3:
                    if (id == 451)
                        id = context.GetHashCode();
                }

                private static Task<int> Whatever() => Task.FromResult(42);
            }
            """);

    [Fact]
    public Task TestNestedTypes() =>
        Verify(
            // lang=C#
            """
            using System;
            using System.Threading.Tasks;
            using Pipelines.Attributes;

            public partial class Example
            {
                protected internal partial class Nested
                {
                    private partial class Nesteder
                    {
                        [Pipeline]
                        public async Task Operation(object context)
                        {
                        Step:
                            Console.WriteLine($"1: {context}");
                        }
                    }
                    
                    [Pipeline]
                    public async Task Operation(object context)
                    {
                    Step:
                        Console.WriteLine($"2: {context}");
                    }
                }

                [Pipeline]
                public async Task Operation(object context)
                {
                Step:
                    Console.WriteLine($"3: {context}");
                }
            }
            """);

    [Fact]
    public Task TestNestedLabel() =>
        Verify(
            // lang=C#
            """
            using System;
            using System.Threading.Tasks;
            using Pipelines.Attributes;

            public partial class Example
            {
                [Pipeline]
                public async Task Operation(object context)
                {
                Step1:
                    var id = 451;

                Step2:
                    Console.WriteLine(context);

                    {
                    Step3:
                        Console.WriteLine(id);
                    }
                }
            }
            """);

    [Fact]
    public Task TestReassigned() =>
        Verify(
            // lang=C#
            """
            using System;
            using System.Threading.Tasks;
            using Pipelines.Attributes;

            public partial class Example
            {
                [Pipeline]
                public static async Task Operation(object context)
                {
                Step1:
                    var id = 451;

                Step2:
                    id = context.GetHashCode();

                Step3:
                    Console.WriteLine(id);
                }
            }
            """);

    [Fact]
    public Task TestDefaultParameters() =>
        Verify(
            // lang=C#
            """
            using System;
            using System.Threading.Tasks;
            using Pipelines.Attributes;

            public partial class Example
            {
                [Pipeline]
                public static async Task Operation(int a, double b, decimal c = 451, string d = "xyz")
                {
                Step1:
                    var id = 451;

                Step2:
                    id = context.GetHashCode();

                Step3:
                    Console.WriteLine(id);
                    Console.WriteLine(a + b + c);
                }
            }
            """);

    [Fact]
    public Task Go() =>
        Verify(
            // lang=C#
            """
            using System;
            using System.Threading.Tasks;
            using Pipelines.Attributes;

            public partial class Example
            {
                [Pipeline]
                public async ValueTask<double> Parse(string input)
                {
                One:
                    Console.WriteLine($"parsing '{input}'");

                Two:
                    var parsed = int.Parse(input);

                Three:
                    Console.WriteLine("askdoaskd");

                    return parsed;
                }
            }
            """);

    private static Task Verify(string source) =>
        Snapshots.Verify<PipelineGenerator>(
            source,
            [typeof(PipelineAttribute).Assembly]);
}
