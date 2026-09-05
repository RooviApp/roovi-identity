using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace ATProto.Primitives;

public sealed partial class DIDRelativeUri : IEquatable<DIDRelativeUri>
{
    [GeneratedRegex(@"^#[^#]+$", RegexOptions.Compiled)]
    private static partial Regex RelativeUriPattern();

    public string Value { get; }

    private DIDRelativeUri([NotNull] string value)
    {
        if (!IsValidDIDRelativeUri(value))
        {
            throw new ArgumentException($"Invalid DIDRelativeUri format: {value}", nameof(value));
        }

        Value = value;
    }

    public static DIDRelativeUri Parse([NotNull] string value)
    {
        return new DIDRelativeUri(value);
    }

    public static bool IsValidDIDRelativeUri([NotNull] string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var isRelative = RelativeUriPattern().IsMatch(value);
        var isRfc3968Uri = RFC3968Uri.IsRfc3986CompliantUri(value);

        return isRelative || isRfc3968Uri;
    }

    public override bool Equals(object? obj)
    {
        if (obj is DIDRelativeUri didRelativeUri)
        {
            return Equals(didRelativeUri);
        }
        return false;
    }

    public bool Equals(DIDRelativeUri? other)
    {
        if (other is null) return false;
        return string.Equals(Value, other.Value, StringComparison.Ordinal);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode(StringComparison.Ordinal);
    }

    public static bool operator ==([NotNull] DIDRelativeUri left, [NotNull] DIDRelativeUri right)
    {
        if (ReferenceEquals(left, right)) return true;
        return left.Equals(right);
    }

    public static bool operator !=([NotNull] DIDRelativeUri left, [NotNull] DIDRelativeUri right)
    {
        return !(left == right);
    }

    public override string ToString()
    {
        return Value;
    }
}
