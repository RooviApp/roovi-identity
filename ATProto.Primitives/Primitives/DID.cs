using ATProto.Primitives.Exceptions;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace ATProto.Primitives;

/// <summary>
/// Value Object reference of AT Protocol's Decentralized Identifier (DID) specification.
/// AT Protocol only allows "plc" and "web" DID methods.
/// Applies Regex to validate the DID format.
/// Can be equated to another DID object, referencing the string primitive.
/// </summary>
public sealed partial class DID : IEquatable<DID>
{
    [GeneratedRegex(@"^did:(web|plc):.+$", RegexOptions.Compiled)]
    private static partial Regex DIDPattern();

    public const string DID_PREFIX = "did:";
    public static readonly int DID_PREFIX_LENGTH = DID_PREFIX.Length;

    public const string DID_PLC_PREFIX = "did:plc:";
    public static readonly int DID_PLC_PREFIX_LENGTH = DID_PLC_PREFIX.Length;
    public static readonly int DID_PLC_LENGTH = 32;

    public const string DID_WEB_PREFIX = "did:web:";
    public static readonly int DID_WEB_PREFIX_LENGTH = DID_WEB_PREFIX.Length;

    public string Value { get; }

    private DID([NotNull] string value)
    {
        if (!IsValidDID(value))
        {
            throw new ArgumentException($"Invalid DID format: {value}", nameof(value));
        }

        Value = value;
    }

    public static DID Create([NotNull] string value)
    {
        return new DID(value);
    }

    public string ExtractMethod()
    {
        var msidSep = Value.IndexOf(':', DID_PREFIX_LENGTH);
        var method = Value[DID_PREFIX_LENGTH..msidSep];
        return method;
    }

    public void AssertValidPlc()
    {
        if (!Value.StartsWith("did:plc:"))
        {
            throw new InvalidDIDException(this, "Invalid did:plc prefix");
        }
        if (Value.Length != DID_PLC_LENGTH)
        {
            throw new InvalidDIDException(this, $"did:plc must be {DID_PLC_LENGTH} characters long");
        }

        foreach (char c in Value[DID_PLC_PREFIX_LENGTH..DID_PLC_LENGTH])
        {
            // If character is not base32 character (a-z2-7), throw
            if (!((c >= 0x61 && c <= 0x7a) || (c >= 0x32 && c <= 0x37)))
            {
                throw new InvalidDIDException(this, $"Invalid character: {c}");
            }
        }
    }

    public void AssertMsid(int start = 0)
    {
        for (var i = start; i < Value.Length; i++)
        {
            var c = Value[i];

            if (
                    (c < 0x61 || c > 0x7a) && // a-z
                    (c < 0x41 || c > 0x5a) && // A-Z
                    (c < 0x30 || c > 0x39) && // 0-9
                    c != 0x2e && // .
                    c != 0x2d && // -
                    c != 0x5f // _
            )
            {
                if (c == 0x3a) // :
                {
                    if (i == Value.Length - 1)
                    {
                        throw new InvalidDIDException(this, "DID cannot end with a \":\"");
                    }
                    continue;
                }

                if (c == 0x25) // percent-encoded
                {
                    if (i + 2 >= Value.Length)
                    {
                        throw new InvalidDIDException(this, $"Incomplete pct-encoded character at position {i}");
                    }

                    c = Value[++i];
                    if ((c < 0x30 || c > 0x39) && (c < 0x41 || c > 0x46)) // 0-9, A-F
                    {
                        throw new InvalidDIDException(this, $"Invalid pct-encoded character at position {i}");
                    }

                    c = Value[++i];
                    if ((c < 0x30 || c > 0x39) && (c < 0x41 || c > 0x46)) // 0-9, A-F
                    {
                        throw new InvalidDIDException(this, $"Invalid pct-encoded character at position {i}");
                    }

                    // There must always be 2 HEXDIG after a "%"
                    if (i >= Value.Length)
                    {
                        throw new InvalidDIDException(this, $"Incomplete pct-encoded character at position {i - 2}");
                    }

                    continue;
                }

                throw new InvalidDIDException (this, $"Disallowed character in DID at position {i}");
            }
        }
    }

    private static bool IsValidDID([NotNull] string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }
        return DIDPattern().IsMatch(value);
    }


    public override bool Equals(object? obj)
    {
        if (obj is DID did)
        {
            return Equals(did);
        }
        return false;
    }

    public bool Equals(DID? other)
    {
        if (other is null) return false;
        return string.Equals(Value, other.Value, StringComparison.Ordinal);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode(StringComparison.Ordinal);
    }

    public static bool operator ==([NotNull] DID left, [NotNull] DID right)
    {
        if (ReferenceEquals(left, right)) return true;
        return left.Equals(right);
    }

    public static bool operator !=([NotNull] DID left, [NotNull] DID right)
    {
        return !(left == right);
    }

    public override string ToString()
    {
        return Value;
    }
}
