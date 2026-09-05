using ATProto.Primitives;
using ATProto.Primitives.Exceptions;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace ATProto;

public sealed partial class Handle : IEquatable<Handle>
{

    [GeneratedRegex(@"^[a-zA-Z0-9.-]*$", RegexOptions.Compiled)]
    private static partial Regex BoringASCIIPattern();

    [GeneratedRegex(@"^[a-zA-Z]", RegexOptions.Compiled)]
    private static partial Regex StartsWithASCIILetterPattern();

    public string Value { get; }

    private Handle([NotNull] string value)
    {
        Value = NormalizeAndEnsureValidHandle(value);
    }

    public static Handle Create([NotNull] string value)
    {
        return new Handle(value);
    }

    private static string NormalizeAndEnsureValidHandle(string handle)
    {
        var normalizedHandle = handle.ToLowerInvariant();
        if (!BoringASCIIPattern().IsMatch(normalizedHandle))
        {
            throw new InvalidHandleException(normalizedHandle, "Disallowed characters in handle (ASCII letters, digits, dashes, periods only)");
        }

        if (normalizedHandle.Length > 253)
        {
            throw new InvalidHandleException(normalizedHandle, "Handle is too long (253 chars max)");
        }

        var labels = normalizedHandle.Split('.');
        if (labels.Length < 2)
        {
            throw new InvalidHandleException(normalizedHandle, "Handle must have at least two parts");
        }

        for (var i = 0; i < labels.Length; i++)
        {
            var label = labels[i];
            if (label.Length < 1)
            {
                throw new InvalidHandleException(normalizedHandle, "Handle parts can not be empty");
            }
            if (label.Length > 63)
            {
                throw new InvalidHandleException(normalizedHandle, "Handle part too long (max 63 chars)");
            }
            if (label.EndsWith('-') || label.StartsWith('-'))
            {
                throw new InvalidHandleException(normalizedHandle, "Handle parts can not start or end with hyphens");
            }
            if (i + 1 == labels.Length && !StartsWithASCIILetterPattern().IsMatch(label))
            {
                throw new InvalidHandleException(normalizedHandle, "Handle final component (TLD) must start with ASCII letter");
            }
        }

        return normalizedHandle;
    }

    public static bool IsResolvedHandle(string input, out DID? resolvedHandle)
    {
        try
        {
            var did = DID.Create(input);
            resolvedHandle = did;
            return true;
        }
        catch (Exception)
        {
            resolvedHandle = null;
            return false;
        }
    }

    public override bool Equals(object? obj)
    {
        if (obj is Handle handle)
        {
            return Equals(handle);
        }
        return false;
    }

    public bool Equals(Handle? other)
    {
        if (other is null) return false;
        return string.Equals(Value, other.Value, StringComparison.Ordinal);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode(StringComparison.Ordinal);
    }

    public static bool operator ==([NotNull] Handle left, [NotNull] Handle right)
    {
        if (ReferenceEquals(left, right)) return true;
        return left.Equals(right);
    }

    public static bool operator !=([NotNull] Handle left, [NotNull] Handle right)
    {
        return !(left == right);
    }

    public override string ToString()
    {
        return Value;
    }
}
