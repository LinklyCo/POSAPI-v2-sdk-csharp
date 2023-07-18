using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Linkly.PosApi.Sdk.DemoPos.Utils;

internal static class Mapping
{
    public static Dictionary<string, string> DictionaryFromType(object @type)
    {
        Dictionary<string, string> response = new();
        if (type == null)
            return response;

        Type t = type.GetType();

        try
        {
            foreach (PropertyInfo pInfo in t.GetProperties().Where(x => x != null))
            {
                var key = pInfo.GetValue(type, Array.Empty<object>());

                if (key == null || (pInfo?.GetCustomAttributes(false)?.FirstOrDefault(a => a is ObsoleteAttribute)) != null)
                    continue;

                string? value = GetMappingValue(key, key.ToString(), key.GetType());
                if (value != null)
                    response.Add(pInfo!.Name, value);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);
        }

        return response;
    }

    private static string? GetMappingValue(object? key, string? value, Type tt)
    {
        if (key is Dictionary<string, string> dic)
        {
            value = string.Join(Environment.NewLine, dic.Select(d => $"{d.Key}: {d.Value};"));
        }
        else if (key is IEnumerable<string> arr)
        {
            value = string.Join(Environment.NewLine, arr);
        }
        else if (tt.IsClass && tt != typeof(string) && !tt.IsArray && key != null)
        {
            var e = DictionaryFromType(key);
            if (e.Count > 0)
                value = string.Join(Environment.NewLine, e.Select(x => $"{x.Key}: {x.Value};"));
        }

        return value;
    }
}
