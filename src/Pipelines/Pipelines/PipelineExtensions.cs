namespace Pipelines;

public static class PipelineExtensions
{
    public static IStep<X, Z> Then<X, Y, Z>(this IStep<X, Y> self, IStep<Y, Z> next) =>
        new CompositeStep<X, Y, Z>(self, next);
}