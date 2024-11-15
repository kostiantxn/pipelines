namespace Pipelines;

/// <summary>
///     A pipeline step that composes two pipeline steps <i>f: <typeparamref name="X"/> -> <typeparamref name="Y"/></i>
///     and <i>g: <typeparamref name="Y"/> -> <typeparamref name="Z"/></i> into a single pipeline
///     that transforms <i><typeparamref name="X"/></i> into <i><typeparamref name="Z"/></i>.
/// </summary>
/// <typeparam name="X">The input type of the first step <i>f</i>.</typeparam>
/// <typeparam name="Y">The output type of the first step <i>f</i> and the input type of the second step <i>g</i>.</typeparam>
/// <typeparam name="Z">The output type of the second step <i>g</i>.</typeparam>
public class CompositeStep<X, Y, Z> : IStep<X, Z>
{
    private readonly IStep<X, Y> _f;
    private readonly IStep<Y, Z> _g;

    public CompositeStep(IStep<X, Y> f, IStep<Y, Z> g)
    {
        _f = f;
        _g = g;
    }

    public async ValueTask<Z> Execute(X input) =>
        await _g.Execute(await _f.Execute(input));

    public override string ToString() =>
        $"{_f} -> {_g}";
}