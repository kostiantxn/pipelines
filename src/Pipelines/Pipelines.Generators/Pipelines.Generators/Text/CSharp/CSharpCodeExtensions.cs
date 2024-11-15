using System;

namespace Pipelines.Generators.Text.CSharp;

/// <summary>
///     Extensions for <see cref="Code"/> to write C# code.
/// </summary>
public static class CSharpCodeExtensions
{
    /// <summary>
    ///     Opens <c>{ ... }</c> block.
    /// </summary>
    /// <param name="code">The code.</param>
    /// <returns>A disposable that closes the block when disposed.</returns>
    public static IDisposable Block(this Code code) =>
        new Scope(
            code,
            x => x.Line('{').Indent(),
            x => x.Dedent().Line('}'));

    /// <inheritdoc cref="Block(Code, string, Action{Code})"/>
    public static Code Block(this Code code, Action<Code> body)
    {
        using (code.Block())
            body(code);

        return code;
    }

    /// <summary>
    ///     Appends a header line and a <c>{ ... }</c> block.
    /// </summary>
    /// <param name="code">The code.</param>
    /// <param name="header">The header line to add before the block.</param>
    /// <param name="body">The action to call inside the block.</param>
    /// <returns>Self.</returns>
    public static Code Block(this Code code, string header, Action<Code> body) =>
        code.Line(header).Block(body);

    /// <inheritdoc cref="Block(Code, Action{Code})"/>
    public static Code Block(this Code code, Action body) =>
        code.Block(_ => body());

    /// <inheritdoc cref="Block(Code, string, Action{Code})"/>
    public static Code Block(this Code code, string header, Action body) =>
        code.Block(header, _ => body());
}