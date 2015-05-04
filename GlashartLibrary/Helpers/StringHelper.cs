using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GlashartLibrary.Helpers
{
    public sealed class StringHelper
    {
        public static string DecodeEncodedNonAsciiCharacters(string value)
        {
            return Regex.Replace(value, @"\\u(?<Value>[a-zA-Z0-9]{4})", m => 
            {
                return ((char)int.Parse(m.Groups["Value"].Value, NumberStyles.HexNumber)).ToString();
            });
        }

    }
}
