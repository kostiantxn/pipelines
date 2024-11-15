using System.Runtime.CompilerServices;
using VerifyTests;

namespace Pipelines.Generators.Tests;

public static class Initializer
{
    [ModuleInitializer]
    public static void Init() =>
        VerifySourceGenerators.Initialize();
}
