using System;

namespace Pipelines.Generators.Text;

/// <summary>
///     A disposable scope that can be used for changing
///     indentation levels or opening and closing blocks
///     of code.
/// </summary>
/// <example>
///     <code>
///         var code = new Code();
///         <br/>
///         using (new Scope(code, x => x.Indent(), x => x.Dedent()))
///         {
///             // The code is indented within this `using` block.
///         }
///     </code>
/// </example>
public class Scope : IDisposable
{
    private readonly Code _code;
    private readonly Action<Code> _close;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Scope"/> class.
    /// </summary>
    /// <param name="code">The code to open a scope for.</param>
    /// <param name="open">The action that opens the scope.</param>
    /// <param name="close">The action that closes the scope.</param>
    /// <remarks>
    ///     The <paramref name="open"/> action is executed immediately, and
    ///     the <paramref name="close"/> action is executed when the scope is
    ///     disposed.
    /// </remarks>
    public Scope(Code code, Action<Code> open, Action<Code> close)
    {
        _code = code;
        _close = close;

        open(_code);
    }

    /// <summary>
    ///     Closes the scope.
    /// </summary>
    public void Dispose() =>
        _close(_code);
}