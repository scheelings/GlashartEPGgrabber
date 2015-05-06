using System.Collections.Generic;

namespace GlashartLibrary
{
    public class Channel
    {
        public string Key { get; set; }
        public string Name { get; set; }
        public List<ChannelLocation> Locations { get; set; }

        public override string ToString()
        {
            return string.Format("{0} ({1}). {2} locations", Name, Key, (Locations != null ? Locations.Count : 0));
        }
    }

    public class ChannelLocation
    {
        public string Name { get; set; }
        public string Url { get; set; }

        public override string ToString()
        {
            return string.Format("{0}:{1}", Name, Url);
        }
    }

    public class ChannelListItem
    {
        public int Number { get; set; }
        public string OriginalName { get; set; }
        public string NewName { get; set; }

        /// <summary>
        /// Gets the name for writing to M3U
        /// </summary>
        /// <returns></returns>
        public string GetName()
        {
            return (string.IsNullOrWhiteSpace(NewName) ? OriginalName : NewName);
        }

        public override string ToString()
        {
            return string.Format("{0}:{1}{2}", Number, OriginalName, (!string.IsNullOrWhiteSpace(NewName) ? string.Concat(" (", NewName, ")") : null));
        }

        /// <summary>
        /// Parses the specified text line.
        /// </summary>
        /// <param name="textLine">The text line.</param>
        /// <returns></returns>
        public static ChannelListItem Parse(string textLine)
        {
            string[] parts = textLine.Split(',');
            if (parts.Length == 2 || parts.Length == 3)
            {
                int number;
                if (int.TryParse(parts[0].Trim(), out number))
                {
                    return new ChannelListItem { Number = number, OriginalName = parts[1].Trim(), NewName = (parts.Length == 3 ? parts[2].Trim() : null) };
                }
            }
            return null;
        }
    }

    public class M3UChannel
    {
        public string Number { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }

        public override string ToString()
        {
            return string.Format("{0}:{1} - {2}", Number, Name, Url);
        }
    }
}
