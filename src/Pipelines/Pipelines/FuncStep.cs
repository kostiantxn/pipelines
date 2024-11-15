namespace Pipelines;

public class FuncStep<X, Y> : IStep<X, Y>
{
    private readonly Func<X, ValueTask<Y>> _func;

    public FuncStep(Func<X, ValueTask<Y>> func) =>
        _func = func;

    public ValueTask<Y> Execute(X input) =>
        _func(input);
}