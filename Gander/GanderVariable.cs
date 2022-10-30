using System;
using System.Runtime.CompilerServices;

namespace Gander;

public struct GanderVariable : IEquatable<GanderVariable>
{
    public object Object;
    public GanderType Type;

    public GanderVariable(object @object, GanderType type)
    {
        Object = @object;
        Type = type;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsNumber() => Type is >= GanderType.I8 and <= GanderType.F64;

    public bool Equals(GanderVariable other)
    {
        return Object.Equals(other.Object) && Type == other.Type;
    }

    public override bool Equals(object obj)
    {
        return obj is GanderVariable other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Object, (int) Type);
    }

    public static bool operator ==(GanderVariable left, GanderVariable right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(GanderVariable left, GanderVariable right)
    {
        return !left.Equals(right);
    }
}