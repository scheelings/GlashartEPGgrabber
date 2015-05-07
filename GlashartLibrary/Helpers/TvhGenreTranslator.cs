using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using log4net;

namespace GlashartLibrary.Helpers
{
    public class TvhGenreTranslator : IGenreTranslator
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(TvhGenreTranslator));

        private const string Language = "TVH";
        private readonly List<Tuple<string, string>> _translations = new List<Tuple<string, string>>(); 

        public void Load(string file)
        {
            if (!File.Exists(file))
            {
                Logger.WarnFormat("Translation file {0} doesn't exist", file);
                return;
            }
            try
            {
                Logger.DebugFormat("Load {0}", file);
                var lines = File.ReadAllLines(file);
                foreach (var line in lines)
                {
                    var splitted = line.Split(';');
                    if (splitted.Length < 2)
                    {
                        Logger.WarnFormat("Failed to convert line as Genre translation: {0}", line);
                        continue;
                    }
                    var glashartGenre = splitted.First();
                    foreach (var tvhGenre in splitted.Skip(1))
                    {
                        Logger.DebugFormat("Add translation: GH {0} -- TVH {1}", glashartGenre, tvhGenre);
                        _translations.Add(new Tuple<string, string>(glashartGenre, tvhGenre));
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to load {0}", file);
            }
        }

        public List<EpgGenre> Translate(List<EpgGenre> genres)
        {
            foreach (var genre in genres.Where(NoTranslation))
            {
                Logger.WarnFormat("Failed to translate genre: {0}", genre.Genre);
            }

            return genres
                .Where(AnyTranslation)
                .SelectMany(GetTranslations)
                .Select(translation => new EpgGenre {
                    Language = Language,
                    Genre = translation
                })
                .ToList();
        }

        private IEnumerable<string> GetTranslations(EpgGenre genre)
        {
            return _translations
                .Where(t => t.Item1 == genre.Genre)
                .Select(t => t.Item2)
                .ToList();
        }

        private bool AnyTranslation(EpgGenre genre)
        {
            return _translations.Any(t => t.Item1 == genre.Genre);
        }

        private bool NoTranslation(EpgGenre genre)
        {
            return !AnyTranslation(genre);
        }
    }
}