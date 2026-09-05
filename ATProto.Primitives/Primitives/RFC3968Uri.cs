using System.Diagnostics.CodeAnalysis;

namespace ATProto.Primitives;

/// <summary>
/// Value Object reference of RFC3968 compliant URI
/// Applies Regex to validate the RFC3968 format.
/// Can be equated to another RFC3968Uri, referencing the string primitive.
/// </summary>
public partial class RFC3968Uri : IEquatable<RFC3968Uri>
{
    public string Value { get; }

    private RFC3968Uri(string value)
    {
        if (!IsRfc3986CompliantUri(value))
        {
            throw new ArgumentException($"Invalid RFC3968Uri format: {value}", nameof(value));
        }
        Value = value;
    }

    public static RFC3968Uri Parse([NotNull] string value)
    {
        return new RFC3968Uri(value);
    }

    public static bool IsRfc3986CompliantUri(string uri) => Uri.TryCreate(uri, UriKind.Absolute, out _);

    public override bool Equals(object? obj)
    {
        if (obj is RFC3968Uri uri)
        {
            return Equals(uri);
        }
        return false;
    }

    public bool Equals(RFC3968Uri? other)
    {
        if (other is null) return false;
        return string.Equals(Value, other.Value, StringComparison.Ordinal);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode(StringComparison.Ordinal);
    }

    public static bool operator ==([NotNull] RFC3968Uri left, [NotNull] RFC3968Uri right)
    {
        if (ReferenceEquals(left, right)) return true;
        return left.Equals(right);
    }

    public static bool operator !=([NotNull] RFC3968Uri left, [NotNull] RFC3968Uri right)
    {
        return !(left == right);
    }

    public override string ToString()
    {
        return Value;
    }
}
