using System.Collections.Generic;

namespace GlashartLibrary.Helpers
{
    public interface IGenreTranslator
    {
        List<EpgGenre> Translate(List<EpgGenre> genres);
    }
}