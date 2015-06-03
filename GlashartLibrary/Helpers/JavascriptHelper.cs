using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace GlashartLibrary.Helpers
{
    public sealed class JavascriptHelper
    {
        private const string StartFunction = "function Rc(";
        private const string ChannelSeparatorRegex = "e.push\\(\"";
        private const string ChannelNameStart = "\"default\":";
        private const string ChannelLocationStart = "b.b=";
        private const string ChannelLocationNameStart = "\"default\":";
        private const string ChannelLocationUrlStart = "b.h=";
        private const string IconEnd = ".png";

        /// <summary>
        /// Parses the channnels from the Tv menu script file
        /// </summary>
        /// <param name="scriptFile">The script file.</param>
        /// <returns></returns>
        public static List<Channel> ParseChannnels(string scriptFile)
        {
            List<Channel> result = new List<Channel>();

            //HACK: this is a dirty way of parsing the channels from the TV menu script, but it get's the job done!
            string script = File.ReadAllText(scriptFile, Encoding.UTF8);

            //Remove everything for the actual function
            int posStart, posEnd;
            posStart = script.IndexOf(StartFunction, StringComparison.InvariantCultureIgnoreCase);
            if (posStart != -1)
            {
                script = script.Substring(posStart);
                //Split script by every e.push(" statement
                var channelParts = Regex.Split(script, ChannelSeparatorRegex, RegexOptions.IgnoreCase).ToList();
                channelParts.RemoveAt(0); // first item does not count

                //Loop through each channel part
                foreach (string channelPart in channelParts)
                {
                    Channel channel = new Channel { Locations = new List<ChannelLocation>() };
                    result.Add(channel);

                    //Key
                    posStart = channelPart.IndexOf("\"");
                    channel.Key = channelPart.Substring(0, posStart);

                    //Name
                    posStart = channelPart.IndexOf(ChannelNameStart);
                    if (posStart != -1)
                    {
                        posStart = posStart + ChannelNameStart.Length;
                        posEnd = channelPart.IndexOf("}", posStart);
                        if (posEnd != -1)
                            channel.Name = RemoveInvalidCharacters(StringHelper.DecodeEncodedNonAsciiCharacters(channelPart.Substring(posStart, posEnd - posStart)));
                    }

                    var iconStart = channelPart.IndexOf(IconEnd);
                    while (iconStart != -1)
                    {
                        var begin = channelPart.LastIndexOf('"', iconStart);
                        if (begin == -1) break;
                        begin++;
                        var end = (iconStart + IconEnd.Length) - begin;
                        var icon = channelPart.Substring(begin, end);
                        channel.Icons.Add(icon);
                        iconStart = channelPart.IndexOf(IconEnd, iconStart + 1);
                    }

                    //Channels
                    int posChannelsStart = posStart;
                    while (true)
                    {
						if (posStart == -1 || posStart >= channelPart.Length)
							break;
						
                        posStart = channelPart.IndexOf(ChannelLocationStart, posStart);
                        if (posStart == -1)
                            break;

                        ChannelLocation location = new ChannelLocation();
                        channel.Locations.Add(location);

                        //Name
                        posStart = channelPart.IndexOf(ChannelLocationNameStart, posStart);
                        if (posStart != -1)
                        {
                            posStart = posStart + ChannelLocationNameStart.Length;
                            posEnd = channelPart.IndexOf("}", posStart);
                            if (posEnd != -1)
                                location.Name = RemoveInvalidCharacters(StringHelper.DecodeEncodedNonAsciiCharacters(channelPart.Substring(posStart, posEnd - posStart)));
                        }

                        //Url
                        posStart = channelPart.IndexOf(ChannelLocationUrlStart, posStart);
                        if (posStart != -1)
                        {
                            posStart = posStart + ChannelLocationUrlStart.Length;
                            posEnd = channelPart.IndexOf(";", posStart);
                            if (posEnd != -1)
                                location.Url = RemoveInvalidCharacters(channelPart.Substring(posStart, posEnd - posStart));
                        }
                    }

                    //Check of we have found any locations
                    if (channel.Locations.Count == 0 && posChannelsStart != -1)
                    {
                        //Check if there is maybe an Url present
                        posStart = channelPart.IndexOf(ChannelLocationUrlStart, posChannelsStart);
                        if (posStart != -1)
                        {
                            posStart = posStart + ChannelLocationUrlStart.Length;
                            posEnd = channelPart.IndexOf(";", posStart);
                            if (posEnd != -1)
                            {
                                ChannelLocation location = new ChannelLocation();
                                channel.Locations.Add(location);
                                location.Url = RemoveInvalidCharacters(channelPart.Substring(posStart, posEnd - posStart));
                            }
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Removes the invalid characters.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        private static string RemoveInvalidCharacters(string value)
        {
            if (value != null) 
            {
                value = value.Replace("'", "");
                value = value.Replace("\"", "");
                value = value.Replace("\n", "");
                value = value.Replace("\r", "");
            }

            return value;
        }
    }
}
