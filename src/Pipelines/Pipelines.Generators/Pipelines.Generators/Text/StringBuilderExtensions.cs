using System.Text;

namespace Pipelines.Generators.Text;

/// <summary>
///     Extensions for <see cref="StringBuilder"/>.
/// </summary>
public static class StringBuilderExtensions
{
    /// <summary>
    ///     Appends the indent the specified number of times if the condition is satisfied.
    /// </summary>
    /// <param name="builder">The builder to append the indent to.</param>
    /// <param name="indent">The indent to append per indentation level.</param>
    /// <param name="level">The indentation level (how many times to append).</param>
    /// <param name="when">The condition to check.</param>
    /// <returns>The specified builder.</returns>
    public static StringBuilder AppendIndent(this StringBuilder builder, string indent, int level, bool when)
    {
        if (when)
            while (--level >= 0)
                builder.Append(indent);

        return builder;
    }
}
