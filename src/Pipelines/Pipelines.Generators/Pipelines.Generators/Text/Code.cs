using System;
using System.Text;

namespace Pipelines.Generators.Text;

/// <summary>
///     A class for writing pretty code.
///     <para/>
///     This class automatically indents code, and provides convenient methods for opening scopes
///     that handle indentation levels.
/// </summary>
/// <example>
///     The following snippet:
///     <para><code>
///     var code = new Code();
///
///     code.Line("public void Hello()")
///         .Line("{")
///         .Scope(() =>
///         {
///             code.Line("Console.WriteLine(\"Bonjour!\");");
///         }
///         .Line("}");
///     </code></para>
///     will produce the following text:
///     <para><code>
///     public void Hello()
///     {
///         Console.WriteLine("Bonjour!");
///     }
///     </code></para>
/// </example>
public class Code
{
    /// <summary>
    ///     The line separator string.
    /// </summary>
    /// <remarks>
    ///     Defaults to <c>"\n"</c>.
    /// </remarks>
    public string Newline { get; set; } = "\n";

    private readonly StringBuilder _builder;
    private readonly string _indent;

    private int _level;
    private bool _newline = true;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Code"/> class.
    /// </summary>
    /// <param name="indent">The indent to append per indentation level.</param>
    public Code(string indent = "    ")
        : this(new(), indent) { }

    /// <summary>
    ///     Initializes a new instance of the <see cref="Code"/> class.
    /// </summary>
    /// <param name="builder">The <see cref="StringBuilder"/> to append code to.</param>
    /// <param name="indent">The indent to append per indentation level.</param>
    public Code(StringBuilder builder, string indent = "    ")
    {
        _builder = builder;
        _indent = indent;
    }

    /// <summary>
    ///     Appends the specified value with a prefix.
    /// </summary>
    /// <remarks>
    ///     If the value consists of multiple lines, each new line will be automatically indented.
    /// </remarks>
    /// <param name="value">The value to append.</param>
    /// <typeparam name="T">The type of value to convert to a string.</typeparam>
    /// <returns>Self.</returns>
    public Code Text<T>(T? value)
    {
        var lines = (value?.ToString() ?? string.Empty).Split(
            ["\r\n", "\r", "\n"], StringSplitOptions.None);

        for (var i = 0; i < lines.Length; ++i)
        {
            _builder.AppendIndent(_indent, _level, when: _newline);
            _builder.Append(lines[i]);

            _newline = i < lines.Length - 1;

            if (_newline)
                _builder.Append(Newline);
        }

        return this;
    }

    /// <summary>
    ///     Appends the specified value and a line separator.
    /// </summary>
    /// <remarks>
    ///     If the value consists of multiple lines, each new line
    ///     will be automatically indented.
    /// </remarks>
    /// <param name="value">The value to append.</param>
    /// <typeparam name="T">The type of value to convert to a string.</typeparam>
    /// <returns>Self.</returns>
    /// <seealso cref="Newline"/>
    public Code Line<T>(T? value) =>
        Text(value).Line();

    /// <summary>
    ///     Appends a line separator.
    /// </summary>
    /// <returns>Self.</returns>
    /// <seealso cref="Newline"/>
    public Code Line()
    {
        _builder.AppendIndent(_indent, _level, when: _newline);
        _builder.Append(Newline);
        _newline = true;
        return this;
    }

    /// <summary>
    ///     Opens a new indentation scope.
    /// </summary>
    /// <returns>A disposable that decreases indentation when disposed.</returns>
    public IDisposable Scope() =>
        new Scope(this, x => x.Indent(), x => x.Dedent());

    /// <summary>
    ///     Calls the specified action inside an indentation scope.
    /// </summary>
    /// <param name="body">The action to call inside the scope.</param>
    /// <returns>Self.</returns>
    /// <seealso cref="Scope()"/>
    public Code Scope(Action<Code> body)
    {
        using (Scope())
            body(this);

        return this;
    }

    /// <inheritdoc cref="Scope(Action{Code})"/>
    public Code Scope(Action body) =>
        Scope(_ => body());

    /// <summary>
    ///     Changes the indentation level by the specified offset.
    /// </summary>
    /// <param name="offset">The value to change the indentation level by.</param>
    /// <returns>Self.</returns>
    public Code Indent(int offset)
    {
        _level = Math.Max(_level + offset, 0);
        return this;
    }

    /// <summary>
    ///     Increases the indentation level by 1.
    /// </summary>
    /// <returns>Self.</returns>
    /// <seealso cref="Indent(int)"/>
    public Code Indent() =>
        Indent(+1);

    /// <summary>
    ///     Decreases the indentation level by 1.
    /// </summary>
    /// <returns>Self.</returns>
    /// <seealso cref="Indent(int)"/>
    public Code Dedent() =>
        Indent(-1);

    /// <inheritdoc/>
    public override string ToString() =>
        _builder.ToString();
}