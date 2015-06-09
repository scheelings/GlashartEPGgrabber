using System;
using System.IO;
using System.Xml.Serialization;
using log4net;

namespace GlashartLibrary.Helpers
{
    public sealed class XmlHelper
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(XmlHelper));

        /// <summary>
        /// Serializes the specified object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj">The object.</param>
        /// <param name="fileName">Name of the file.</param>
        public static void Serialize<T>(T obj, string fileName)
        {
            var xs = new XmlSerializer(typeof(T));
            using (var stream = new StreamWriter(fileName))
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
            try
            {
                var xs = new XmlSerializer(typeof (T));
                using (var stream = new StreamReader(fileName))
                {
                    return (T) xs.Deserialize(stream);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex,"Failed to read file {0} as XML for {1}", fileName, typeof(T).Name);
                return default(T);
            }
        }
    }
}
