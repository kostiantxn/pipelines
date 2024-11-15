using System;
using System.Threading.Tasks;
using Pipelines.Attributes;

#pragma warning disable CS0164 // This label has not been referenced
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

namespace Pipelines.Generators.Sample;

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
        return parsed;
    }

    public static partial class ParsePipeline;
}