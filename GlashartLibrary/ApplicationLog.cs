using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlashartLibrary
{
    public class ApplicationLog
    {
        #region Members
        private static readonly object lockObject = new object();
        private static ILog DefaultLog = null;
        #endregion Members

        #region Constructor
        /// <summary>
        /// Constructor to prevent instantiation of this class
        /// </summary>
        private ApplicationLog()
        {
        }

        /// <summary>
        /// Constructor for ApplicationLog.  
        /// </summary>
        static ApplicationLog()
        {
            try
            {
                if (DefaultLog == null)
                {
                    lock (lockObject)
                    {
                        log4net.Config.XmlConfigurator.Configure();
                        DefaultLog = LogManager.GetLogger("ApplicationLog");
                    }
                }
            }
            catch { }
        }
        #endregion Constructor

        /// <summary>
        ///  Write at the Error level to the event log and/or tracing file.
        /// </summary>
        /// <param name="e">The exception to log</param>
        /// <param name="catchInfo">Extra catchinfo</param>
        public static void WriteError(System.Exception e)
        {
            WriteError(e, string.Empty);
        }

        /// <summary>
        /// Write at the Error level to the event log and/or tracing file.
        /// </summary>
        /// <param name="e">The exception to log</param>
        /// <param name="message">The message.</param>
        /// <param name="args">The arguments.</param>
        public static void WriteError(System.Exception e, string message, params object[] args)
        {
            DefaultLog.Error(string.Format(message, args), e);
        }

        /// <summary>
        ///     Write at the Error level to the event log and/or tracing file.
        ///     <param name="message">The text to write to the log file or event log.</param>
        /// </summary>
        public static void WriteError(string message, params object[] args)
        {
            DefaultLog.ErrorFormat(message, args);
        }

        /// <summary>
        ///     Write at the Warning level to the event log and/or tracing file.
        ///     <param name="message">The text to write to the log file or event log.</param>
        /// </summary>
        public static void WriteWarning(string message, params object[] args)
        {
            DefaultLog.WarnFormat(message, args);
        }

        /// <summary>
        ///     Write at the Info level to the event log and/or tracing file.
        ///     <param name="message">The text to write to the log file or event log.</param>
        /// </summary>
        public static void WriteInfo(string message, params object[] args)
        {
            DefaultLog.InfoFormat(message, args);
        }

        /// <summary>
        ///     Write at the Debug level to the event log and/or tracing file.
        ///     <param name="message">The text to write to the log file or event log.</param>
        /// </summary>
        public static void WriteDebug(string message, params object[] args)
        {
            DefaultLog.DebugFormat(message, args);
        }
    }
}
