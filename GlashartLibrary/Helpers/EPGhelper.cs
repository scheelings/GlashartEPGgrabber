using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace GlashartLibrary.Helpers
{
    public sealed class EPGhelper
    {
        private const string EpgFileNameFormat = "epgdata.{datepart}.{daypart}.json.gz";

        /// <summary>
        /// Downloads the EPG (compressed) files.
        /// </summary>
        /// <param name="baseUrl">The base URL.</param>
        /// <param name="localFolder">The local folder.</param>
        /// <param name="numberOfDays">Number of days to download</param>
        public static void DownloadEPGfiles(string baseUrl, string localFolder, int numberOfDays)
        {
            //EPG url example: http://w.zt6.nl/epgdata/epgdata.20141128.1.json.gz
            DateTime date = DateTime.Today;
            for (int dayNr = 0; dayNr < numberOfDays; dayNr++)
            {
                //EPG is downloaded in 8 parts
                for (int dayPart = 0; dayPart < 8; dayPart++)
                {
                    string url = EpgFileNameFormat.Replace("{datepart}", date.AddDays(dayNr).ToString("yyyyMMdd"));
                    url = url.Replace("{daypart}", dayPart.ToString());

                    string localFile = Path.Combine(localFolder, url);
                    url = string.Concat(baseUrl, url);

                    //Download the file
                    try
                    {
                        HttpDownloader.DownloadBinaryFile(url, localFile);
                    }
                    catch (Exception err)
                    {
                        ApplicationLog.WriteError(err, "Unable to download EPG for URL '{0}'", url);
                    }
                }
            }
        }

        /// <summary>
        /// Decompresses the EPG files.
        /// </summary>
        /// <param name="localFolder">The local folder.</param>
        /// <param name="numberOfDays">Number of days to decompress</param>
        public static void DecompressEPGfiles(string localFolder, int numberOfDays)
        {
            DateTime date = DateTime.Today;
            for (int dayNr = 0; dayNr < numberOfDays; dayNr++)
            {
                //EPG is in 8 parts
                for (int dayPart = 0; dayPart < 8; dayPart++)
                {
                    string name = EpgFileNameFormat.Replace("{datepart}", date.AddDays(dayNr).ToString("yyyyMMdd"));
                    name = name.Replace("{daypart}", dayPart.ToString());
                    string compressedFile = Path.Combine(localFolder, name);
                    string uncompressedFile = compressedFile.Replace(".gz", "");

                    //Decompress the file
                    try
                    {
                        if (File.Exists(compressedFile))
                        {
                            CompressionHelper.Decompress(compressedFile, uncompressedFile);
                        }
                        else
                        {
                            ApplicationLog.WriteDebug("EPG file {0} not found to decompress", name);
                        }
                    }
                    catch (Exception err)
                    {
                        ApplicationLog.WriteError(err, "Unable to decompress EPG file '{0}'", name);
                    }
                }
            }
        }

        /// <summary>
        /// Reads the EPG files into a list of EPG objects
        /// </summary>
        /// <param name="localFolder">The local folder.</param>
        /// <param name="numberOfDays">Number of days to decompress</param>
        public static List<EpgChannel> ReadEPGfiles(string localFolder, int numberOfDays)
        {
            List<EpgChannel> result = new List<EpgChannel>();

            DateTime date = DateTime.Today;
            for (int dayNr = 0; dayNr < numberOfDays; dayNr++)
            {
                //EPG is in 8 parts
                for (int dayPart = 0; dayPart < 8; dayPart++)
                {
                    string name = EpgFileNameFormat.Replace("{datepart}", date.AddDays(dayNr).ToString("yyyyMMdd"));
                    name = name.Replace("{daypart}", dayPart.ToString());
                    string compressedFile = Path.Combine(localFolder, name);
                    string uncompressedFile = compressedFile.Replace(".gz", "");

                    //Read the JSON file
                    try
                    {
                        if (File.Exists(uncompressedFile))
                        {
                            var converter = new ExpandoObjectConverter();
                            dynamic json = JsonConvert.DeserializeObject<ExpandoObject>(File.ReadAllText(uncompressedFile), converter);
                            if (json != null)
                            {
                                foreach (var channelName in json)
                                {
                                    EpgChannel channel = result.FirstOrDefault(c => c.Channel.Equals((string)channelName.Key, StringComparison.InvariantCultureIgnoreCase));
                                    if (channel == null)
                                    {
                                        channel = new EpgChannel { Programs = new List<EpgProgram>() };
                                        result.Add(channel);

                                        channel.Channel = (string)channelName.Key;
                                    }

                                    //Add programms
                                    foreach (var program in channelName.Value)
                                    {
                                        EpgProgram prog = new EpgProgram();

                                        foreach (var programProperty in program)
                                        {
                                            string key = GetJsonValue(programProperty.Key);
                                            string value = GetJsonValue(programProperty.Value);
                                            if (!string.IsNullOrWhiteSpace(key) && !string.IsNullOrWhiteSpace(value))
                                            {
                                                switch (key.ToLower())
                                                {
                                                    case "id":
                                                        prog.Id = value;
                                                        break;
                                                    case "name":
                                                        prog.Name = StringHelper.DecodeEncodedNonAsciiCharacters(value);
                                                        break;
                                                    case "start":
                                                        prog.SetStart(value);
                                                        break;
                                                    case "end":
                                                        prog.SetEnd(value);
                                                        break;
                                                    case "disablerestart":
                                                        //Ignore
                                                        break;
                                                    default:
                                                        //I'm curious which other data is present
                                                        prog.OtherData = string.Format("{0}{1}={2};", prog.OtherData, key, value);
                                                        break;
                                                }
                                            }
                                        }

                                        //Add the program when it does not exist yet
                                        if (!channel.Programs.Any(p => p.Start == prog.Start && p.End == prog.End))
                                            channel.Programs.Add(prog);
                                    }
                                }
                            }
                        }
                        else
                        {
                            ApplicationLog.WriteDebug("EPG file {0} not found to read", name.Replace(".gz", ""));
                        }
                    }
                    catch (Exception err)
                    {
                        ApplicationLog.WriteError(err, "Unable to decompress EPG file '{0}'", name);
                    }
                }
            }

            //Order all programs
            foreach (var channel in result)
                channel.Programs = channel.Programs.OrderBy(c => c.Start).ToList();

            return result;
        }
        /// <summary>
        /// Gets the json value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        private static string GetJsonValue(dynamic value)
        {
            if (value == null)
                return null;
            else
                return value.ToString();
        }

        /// <summary>
        /// Cleans up EPG in local folder
        /// </summary>
        /// <param name="localFolder">The local folder.</param>
        /// <param name="olderThanDays">Delete files older than x days.</param>
        public static void CleanUpEPG(string localFolder, int olderThanDays)
        {
            foreach (FileInfo file in new DirectoryInfo(localFolder).GetFiles())
            {
                if (DateTime.Today.Subtract(file.LastWriteTime).TotalDays > olderThanDays)
                    file.Delete();
            }
        }

        public static string DownloadDetails(string id)
        {
            if (string.IsNullOrWhiteSpace(id) || id.Length < 2)
            {
                ApplicationLog.WriteDebug("No valid ID to download details");
                return null;
            }
            try
            {
                var dir = id.Substring(id.Length - 2, 2);
                var url = string.Format("{0}{1}/{2}.json", Main.Settings.EpgURL, dir, id);
                ApplicationLog.WriteDebug("Try to download {0}", url);
                var data = HttpDownloader.DownloadTextFile(url);
                ApplicationLog.WriteDebug("Downloaded details: {0}", data);
            }
            catch (Exception)
            {
                ApplicationLog.WriteDebug("No detailed data found for id {0}");
            }
            return null;
        }
    }
}

