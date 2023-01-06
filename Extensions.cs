using System.Collections.Generic;
using System.Text.Json;

namespace srra;

public static class Extensions
{
    public static Dictionary<string, JsonElement>? GetNestedJsonObject(this JsonElement jsonElement)
    {
        return JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(JsonSerializer.Serialize(jsonElement));
    }
}