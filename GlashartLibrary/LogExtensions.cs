using System;
using log4net;

namespace GlashartLibrary
{
    public static class LogExtensions
    {
        public static void Error(this ILog log, Exception ex, string format, params object[] args)
        {
            log.Error(string.Format(format,args), ex);
        } 
    }
}