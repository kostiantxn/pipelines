using System;
using Pipelines;
using Pipelines.Generators.Sample;

Console.WriteLine("Hello World!");

var example = new Example();

var pipeline = new Example.ParsePipeline.OneStep(example)
     .Then(new Example.ParsePipeline.TwoStep(example))
     .Then(new Example.ParsePipeline.ThreeStep(example));

var result = await pipeline.Execute(new() { input = "1234" });

Console.WriteLine(result);
