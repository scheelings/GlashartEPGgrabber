using GlashartLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using log4net;
using Microsoft.SqlServer.Server;

namespace GlashartEPGgrabber
{
    public class Program
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(Program));

        private const string CommandLineArgument_DownloadTvMenu = "/dl-tvmenu";
        private const string CommandLineArgument_DecompressTvMenu = "/unzip-tvmenu";
        private const string CommandLineArgument_DownloadTvMenuScript = "/dl-tvscript";
        private const string CommandLineArgument_DecompressTvMenuScript = "/unzip-tvscript";
        private const string CommandLineArgument_GenerateChannelsFile = "/channels";
        private const string CommandLineArgument_GenerateM3Ufile = "/m3u";
        private const string CommandLineArgument_DownloadEPG = "/dl-epg";
        private const string CommandLineArgument_DecompressEPG = "/unzip-epg";
        private const string CommandLineArgument_DownloadDetails = "/dl-details";
        private const string CommandLineArgument_XmlTV = "/xmltv";

        private const string CommandLineArgument_AllM3U = "/all-m3u";
        private const string CommandLineArgument_AllXmlTv = "/all-xmltv";
        private const string CommandLineArgument_All = "/all";

        private const string CommandLineArgument_ConvertM3U = "/convert-m3u";

        private const string CommandLineArgument_IniSettings = "/ini-settings";
        
        private static bool ShowHelp = true;
        private static bool DownloadTvMenu = false;
        private static bool DecompressTvMenu = false;
        private static bool DownloadTvMenuScript = false;
        private static bool DecompressTvMenuScript = false;
        private static bool GenerateChannelsFile = false;
        private static bool GenerateM3Ufile = false;
        private static bool DownloadEPG = false;
        private static bool DecompressEPG = false;
        private static bool XmlTV = false;
        private static bool ConvertM3U = false;
        private static bool IniSettings = false;
        private static bool DownloadDetails = false;

        /// <summary>
        /// Main entry of the console application
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        public static void Main(string[] args)
        {
            CheckCommandLineArguments(args);

            Logger.Info("Glashart EPG Grabber (by Dennieku, JanSaris)");
            Logger.Info("----------------------------------");

            if (ShowHelp)
            {
                using (var stream = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("GlashartEPGgrabber.help.txt")))
                {
                    var help = stream.ReadToEnd();
                    Console.WriteLine(help);
                }
                ExitApplication();
            }
            else
            {
                GlashartLibrary.Main.Settings = LoadSettings();
                if (DownloadTvMenu)
                    GlashartLibrary.Main.DownloadTvMenu();
                if (DecompressTvMenu)
                    GlashartLibrary.Main.DecompressTvMenu();
                if (DownloadTvMenuScript)
                    GlashartLibrary.Main.DownloadTvMenuScript();
                if (DecompressTvMenuScript)
                    GlashartLibrary.Main.DecompressTvMenuScript();
                List<Channel> channels = null;
                if (GenerateChannelsFile)
                    channels = GlashartLibrary.Main.GenerateChannelXmlFile();
                if (GenerateM3Ufile)
                    channels = GlashartLibrary.Main.GenerateM3Ufile(channels);
                if (DownloadEPG)
                    GlashartLibrary.Main.DownloadEPGfiles();
                if (DecompressEPG)
                    GlashartLibrary.Main.DecompressEPGfiles();
                if (XmlTV)
                {
                    var epgData = GlashartLibrary.Main.ReadEpgFromFiles();
                    channels = GlashartLibrary.Main.ReadChannelList();
                    if (DownloadDetails)
                        epgData = GlashartLibrary.Main.DownloadDetails(epgData, channels);
                    GlashartLibrary.Main.GenerateXmlTv(epgData, channels);
                }
                
                if (ConvertM3U)
                    GlashartLibrary.Main.ConvertM3Ufile();

                ExitApplication();
            }
        }

        private static void ExitApplication()
        {
            Logger.Info("Glashart EPG Grabber ended");
#if DEBUG
            Console.WriteLine("Press any key to exit...");
            Console.Read();
#endif
        }

        /// <summary>
        /// Checks the command line arguments
        /// </summary>
        /// <param name="args">The arguments.</param>
        private static void CheckCommandLineArguments(string[] args)
        {
            if (args == null || args.Length == 0)
                return;

            //Check the arguments
            foreach (string arg in args)
            {
                if (arg.Trim().Equals(CommandLineArgument_DownloadTvMenu))
                {
                    DownloadTvMenu = true;
                    ShowHelp = false;
                }
                else if (arg.Trim().Equals(CommandLineArgument_DecompressTvMenu))
                {
                    DecompressTvMenu = true;
                    ShowHelp = false;
                }
                else if (arg.Trim().Equals(CommandLineArgument_DownloadTvMenuScript))
                {
                    DownloadTvMenuScript = true;
                    ShowHelp = false;
                }
                else if (arg.Trim().Equals(CommandLineArgument_DecompressTvMenuScript))
                {
                    DecompressTvMenuScript = true;
                    ShowHelp = false;
                }
                else if (arg.Trim().Equals(CommandLineArgument_GenerateChannelsFile))
                {
                    GenerateChannelsFile = true;
                    ShowHelp = false;
                }
                else if (arg.Trim().Equals(CommandLineArgument_GenerateM3Ufile))
                {
                    GenerateM3Ufile = true;
                    ShowHelp = false;
                }
                else if (arg.Trim().Equals(CommandLineArgument_DownloadEPG))
                {
                    DownloadEPG = true;
                    ShowHelp = false;
                }
                else if (arg.Trim().Equals(CommandLineArgument_DownloadDetails))
                {
                    DownloadDetails = true;
                }
                else if (arg.Trim().Equals(CommandLineArgument_DecompressEPG))
                {
                    DecompressEPG = true;
                    ShowHelp = false;
                }
                else if (arg.Trim().Equals(CommandLineArgument_XmlTV))
                {
                    XmlTV = true;
                    ShowHelp = false;
                }
                else if (arg.Trim().Equals(CommandLineArgument_AllM3U))
                {
                    DownloadTvMenu = true;
                    DecompressTvMenu = true;
                    DownloadTvMenuScript = true;
                    DecompressTvMenuScript = true;
                    GenerateChannelsFile = true;
                    GenerateM3Ufile = true;

                    ShowHelp = false;
                }
                else if (arg.Trim().Equals(CommandLineArgument_AllXmlTv))
                {
                    DownloadEPG = true;
                    DecompressEPG = true;
                    DownloadDetails = true;
                    XmlTV = true;

                    ShowHelp = false;
                }
                else if (arg.Trim().Equals(CommandLineArgument_All))
                {
                    DownloadTvMenu = true;
                    DecompressTvMenu = true;
                    DownloadTvMenuScript = true;
                    DecompressTvMenuScript = true;
                    GenerateChannelsFile = true;
                    GenerateM3Ufile = true;

                    DownloadEPG = true;
                    DecompressEPG = true;
                    DownloadDetails = true;
                    XmlTV = true;

                    ShowHelp = false;
                }
                else if (arg.Trim().Equals(CommandLineArgument_ConvertM3U))
                {
                    ConvertM3U = true;
                    ShowHelp = false;
                }
                else if (arg.Trim().Equals(CommandLineArgument_IniSettings))
                {
                    IniSettings = true;
                }
            }
        }

        private static ISettings LoadSettings()
        {
            if(!IniSettings) return new ConfigSettings();
            LogSetup.Setup();
            var settings = new IniSettings();
            settings.Load();
            if (!string.IsNullOrWhiteSpace(settings.LogLevel))
            {
                LogSetup.ChangeLogLevel(settings.LogLevel);
            }
            return settings;
        }
    }
}
