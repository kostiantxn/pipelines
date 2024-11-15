namespace Pipelines;

/// <summary>
///     A pipeline step that transforms input of type <i><typeparamref name="I"/></i>
///     into output of type <i><typeparamref name="O"/></i>.
/// </summary>
/// <typeparam name="I">The type of input.</typeparam>
/// <typeparam name="O">The type of output.</typeparam>
public interface IStep<in I, O>
{
    /// <summary>
    ///     Executes the step.
    /// </summary>
    /// <param name="input">The input to execute the step with.</param>
    /// <returns>The step output.</returns>
    ValueTask<O> Execute(I input);
}