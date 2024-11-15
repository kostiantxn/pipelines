namespace Pipelines.Attributes;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class PipelineForAttribute : Attribute
{
    public string Name { get; }

    public PipelineForAttribute(string name) =>
        Name = name;
}