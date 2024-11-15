using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using VerifyXunit;

namespace Pipelines.Generators.Tests;

/// <summary>
///     A helper class for verifying snapshots.
/// </summary>
public static class Snapshots
{
    /// <summary>
    ///     The directory where to put snapshots.
    /// </summary>
    private const string Directory = "Snapshots";

    /// <inheritdoc cref="Verify{T}(string, IEnumerable{Assembly}, string, string?)"/>
    public static Task Verify<T>(string source, string assembly = "Tests", string? directory = null)
        where T : IIncrementalGenerator, new() =>
        Verify<T>(source, Array.Empty<Assembly>(), assembly, directory);

    /// <summary>
    ///     Compiles the provided C# source code and runs the specified
    ///     <see cref="IIncrementalGenerator"/> to verify the produced output as snapshots.
    /// </summary>
    /// <param name="source">The C# source code to compile.</param>
    /// <param name="assemblies">The list of assemblies to add to the compilation.</param>
    /// <param name="assembly">The name of the compiled assembly.</param>
    /// <param name="directory">The path to the subdirectory to save snapshots to (defaults to generator name).</param>
    public static Task Verify<T>(string source, IEnumerable<Assembly> assemblies, string assembly = "Tests", string? directory = null)
        where T : IIncrementalGenerator, new()
    {
        var references = AppDomain.CurrentDomain.GetAssemblies()
            .Where(x => !x.IsDynamic && !string.IsNullOrWhiteSpace(x.Location))
            .Append(typeof(T).Assembly)
            .Concat(assemblies)
            .Select(x => MetadataReference.CreateFromFile(x.Location))
            .Distinct();

        var compilation = CSharpCompilation.Create(assembly, options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
            .AddSyntaxTrees(CSharpSyntaxTree.ParseText(source))
            .AddReferences(references);

        var driver = CSharpGeneratorDriver.Create(new T().AsSourceGenerator());

        return Verifier
            .Verify(driver.RunGenerators(compilation))
            .UseDirectory(Path.Join(Directory, directory ?? typeof(T).Name));
    }
}
