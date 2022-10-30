using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection.Metadata;

namespace Gander;

public static class Extensions
{
    public static void AddOrUpdate(this Dictionary<string, GanderVariable> dictionary, string key, GanderVariable value)
    {
        if (!dictionary.TryAdd(key, value))
            dictionary[key] = value;
    }
}