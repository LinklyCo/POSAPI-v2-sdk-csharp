using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linkly.PosApi.Sdk.DemoPos.Utils;

static class Converter
{
    internal static int ToInt(this string value)
    {
        _ = decimal.TryParse(value, out decimal decimalValue);
        return Convert.ToInt32(decimalValue * 100);
    }

    internal static uint ToUInt(this string value)
    {
        _ = decimal.TryParse(value, out decimal decimalValue);
        return Convert.ToUInt32(decimalValue * 100);
    }

    internal static byte ToByte(this string value)
    {
        _ = byte.TryParse(value, out byte byteValue);
        return byteValue;
    }
}
