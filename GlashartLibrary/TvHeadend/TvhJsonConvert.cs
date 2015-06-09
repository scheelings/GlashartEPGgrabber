using System.IO; 
using Newtonsoft.Json;

namespace GlashartLibrary.TvHeadend
{
    public class TvhJsonConvert
    {
        public static string Serialize(object obj)
        {
            var sw = new StringWriter();
            var textwriter = new TvhJsonTextWriter(sw);
            var serializer = new JsonSerializer { Formatting = Formatting.Indented };
            serializer.Serialize(textwriter,obj);
            return sw.ToString();
        } 
    }
}