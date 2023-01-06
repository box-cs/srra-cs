using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace srra;

public static class Extensions
{
    public static Dictionary<string, JsonElement>? GetNestedJsonObject(this JsonElement jsonElement)
    {
        return JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(JsonSerializer.Serialize(jsonElement));
    }
}