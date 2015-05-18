using System.Globalization;
using System.Text.RegularExpressions;

namespace GlashartLibrary.Helpers
{
    public static class StringHelper
    {
        public static string DecodeEncodedNonAsciiCharacters(string value)
        {
            return Regex.Replace(value, @"\\u(?<Value>[a-zA-Z0-9]{4})", m => ((char)int.Parse(m.Groups["Value"].Value, NumberStyles.HexNumber)).ToString(CultureInfo.InvariantCulture));
        }

    }
}
