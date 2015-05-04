using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace GlashartLibrary.Helpers
{
    public sealed class XmlHelper
    {
        /// <summary>
        /// Serializes the specified object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj">The object.</param>
        /// <param name="fileName">Name of the file.</param>
        public static void Serialize<T>(T obj, string fileName)
        {
            XmlSerializer xs = new XmlSerializer(typeof(T));
            using (StreamWriter stream = new StreamWriter(fileName))
            {
                xs.Serialize(stream, obj);
            }
        }

        /// <summary>
        /// Deserializes the specified file name to the specified object type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fileName">Name of the file.</param>
        /// <returns></returns>
        public static T Deserialize<T>(string fileName)
        {
            XmlSerializer xs = new XmlSerializer(typeof(T));
            using (StreamReader stream = new StreamReader(fileName))
            {
                return (T)xs.Deserialize(stream);
            }
        }
    }
}
