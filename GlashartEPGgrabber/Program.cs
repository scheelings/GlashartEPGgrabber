using GlashartLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using GlashartLibrary.Helpers;
using GlashartLibrary.IO;
using GlashartLibrary.Settings;
using log4net;

namespace GlashartEPGgrabber
{
    public class Program
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(Program));

        private static IWebDownloader _webDownloader = new HttpWebDownloader();

        private const string CommandLineArgument_DownloadTvMenu = "/dl-tvmenu";
        private const string CommandLineArgument_DecompressTvMenu = "/unzip-tvmenu";
        private const string CommandLineArgument_DownloadTvMenuScript = "/dl-tvscript";
        private const string CommandLineArgument_DecompressTvMenuScript = "/unzip-tvscript";
        private const string CommandLineArgument_GenerateChannelsFile = "/channels";
        private const string CommandLineArgument_GenerateM3Ufile = "/m3u";
        private const string CommandLineArgument_DownloadChannelIcons = "/dl-icons";
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
        private static bool DownloadChannelIcons = false;
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
                var main = Initialize();
                if (DownloadTvMenu)
                    main.DownloadTvMenu();
                if (DecompressTvMenu)
                    main.DecompressTvMenu();
                if (DownloadTvMenuScript)
                    main.DownloadTvMenuScript();
                if (DecompressTvMenuScript)
                    main.DecompressTvMenuScript();
                List<Channel> channels = null;
                if (GenerateChannelsFile)
                    channels = main.GenerateChannelXmlFile();
                if (GenerateM3Ufile)
                    channels = main.GenerateM3Ufile(channels);
                if (DownloadChannelIcons)
                    main.DownloadChannelIcons(channels);
                if (DownloadEPG)
                    main.DownloadEpGfiles();
                if (DecompressEPG)
                    main.DecompressEpGfiles();
                if (XmlTV)
                {
                    var epgData = main.ReadEpgFromFiles();
                    channels = main.ReadChannelList();
                    if (DownloadDetails)
                        epgData = main.DownloadDetails(epgData, channels);
                    main.GenerateXmlTv(epgData, channels);
                }
                
                if (ConvertM3U)
                    main.ConvertM3Ufile();

                Teardown();
                ExitApplication();
            }
        }

        private static CachedWebDownloader _cachedWebDownloader;

        private static Main Initialize()
        {
            var settings = LoadSettings();
            _cachedWebDownloader = new CachedWebDownloader(settings.DataFolder, _webDownloader);
            _cachedWebDownloader.LoadCache();
            var fileDownloader = new FileDownloader(_webDownloader);
            var downloader = new Downloader(_cachedWebDownloader, fileDownloader);
            var translator = GetTranslator(settings);
            return new Main(settings, downloader, translator);
        }

        private static IGenreTranslator GetTranslator(ISettings settings)
        {
            IGenreTranslator translator = null;
            if (!string.IsNullOrWhiteSpace(settings.TvhGenreTranslationsFile))
            {
                var tvhTranslator = new TvhGenreTranslator();
                tvhTranslator.Load(settings.TvhGenreTranslationsFile);
                translator = tvhTranslator;
            }
            return translator;
        }

        private static void Teardown()
        {
            if (_cachedWebDownloader != null)
            {
                _cachedWebDownloader.SaveCache();
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
                else if (arg.Trim().Equals(CommandLineArgument_DownloadChannelIcons))
                {
                    DownloadChannelIcons = true;
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
                else if (arg.Trim().Equals("/no-iptv"))
                {
                    _webDownloader = new NullWebDownloader();
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
