using GlashartLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using GlashartLibrary.Helpers;
using GlashartLibrary.IO;
using GlashartLibrary.Settings;
using log4net;
using log4net.Config;

namespace GlashartEPGgrabber
{
    public class Program
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(Program));

        private static IWebDownloader _webDownloader = new HttpWebDownloader();

        private const string CommandLineArgumentDownloadTvMenu = "/dl-tvmenu";
        private const string CommandLineArgumentDecompressTvMenu = "/unzip-tvmenu";
        private const string CommandLineArgumentDownloadTvMenuScript = "/dl-tvscript";
        private const string CommandLineArgumentDecompressTvMenuScript = "/unzip-tvscript";
        private const string CommandLineArgumentGenerateChannelsFile = "/channels";
        private const string CommandLineArgumentGenerateM3Ufile = "/m3u";
        private const string CommandLineArgumentDownloadChannelIcons = "/dl-icons";
        private const string CommandLineArgumentDownloadEpg = "/dl-epg";
        private const string CommandLineArgumentDecompressEpg = "/unzip-epg";
        private const string CommandLineArgumentDownloadDetails = "/dl-details";
        private const string CommandLineArgumentXmlTv = "/xmltv";
        private const string CommandLineArgumentTVheadend = "/tvh";

        private const string CommandLineArgumentAllM3U = "/all-m3u";
        private const string CommandLineArgumentAllXmlTv = "/all-xmltv";
        private const string CommandLineArgumentAll = "/all";

        private const string CommandLineArgumentConvertM3U = "/convert-m3u";
        private const string CommandLineArgumentM3UtoTVheadend = "/m3u-to-tvh";

        private const string CommandLineArgumentIniSettings = "/ini-settings";
        
        private static bool _showHelp = true;
        private static bool _downloadTvMenu;
        private static bool _decompressTvMenu;
        private static bool _downloadTvMenuScript;
        private static bool _decompressTvMenuScript;
        private static bool _generateChannelsFile;
        private static bool _generateM3Ufile;
        private static bool _downloadChannelIcons;
        private static bool _downloadEpg;
        private static bool _decompressEpg;
        private static bool _xmlTv;
        private static bool _convertM3U;
        private static bool _iniSettings;
        private static bool _downloadDetails;
        private static bool _generateTVheadend;
        private static bool _convertM3uToTVheadend;

        /// <summary>
        /// Main entry of the console application
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        public static void Main(string[] args)
        {
            CheckCommandLineArguments(args);

            Logger.Info("Glashart EPG Grabber (by Dennieku, JanSaris)");
            Logger.Info("----------------------------------");

            if (_showHelp)
            {
                // ReSharper disable once AssignNullToNotNullAttribute
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
                if (_downloadTvMenu)
                    main.DownloadTvMenu();
                if (_decompressTvMenu)
                    main.DecompressTvMenu();
                if (_downloadTvMenuScript)
                    main.DownloadTvMenuScript();
                if (_decompressTvMenuScript)
                    main.DecompressTvMenuScript();
                List<Channel> channels = null;
                if (_generateChannelsFile)
                    channels = main.GenerateChannelXmlFile();
                if (_generateM3Ufile)
                    channels = main.GenerateM3Ufile(channels);
                if (_downloadChannelIcons)
                    main.DownloadChannelIcons(channels);
                if (_downloadEpg)
                    main.DownloadEpGfiles();
                if (_decompressEpg)
                    main.DecompressEpGfiles();
                if (_xmlTv)
                {
                    var epgData = main.ReadEpgFromFiles();
                    channels = main.ReadChannelList();
                    if (_downloadDetails)
                        epgData = main.DownloadDetails(epgData, channels);
                    main.GenerateXmlTv(epgData, channels);
                }
                if (_generateTVheadend)
                {
                    main.UpdateTvHeadend(channels);
                }

                if (_convertM3uToTVheadend)
                {
                    main.ConvertM3UtoTVheadend();
                }
                
                if (_convertM3U)
                    main.ConvertM3Ufile();

                Teardown();
                ExitApplication();
            }
        }

        private static CachedWebDownloader _cachedWebDownloader;

        private static Main Initialize()
        {
            var settings = LoadSettings();
            _cachedWebDownloader = new CachedWebDownloader(settings, _webDownloader);
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
                if (arg.Trim().Equals(CommandLineArgumentDownloadTvMenu))
                {
                    _downloadTvMenu = true;
                    _showHelp = false;
                }
                else if (arg.Trim().Equals(CommandLineArgumentDecompressTvMenu))
                {
                    _decompressTvMenu = true;
                    _showHelp = false;
                }
                else if (arg.Trim().Equals(CommandLineArgumentDownloadTvMenuScript))
                {
                    _downloadTvMenuScript = true;
                    _showHelp = false;
                }
                else if (arg.Trim().Equals(CommandLineArgumentDecompressTvMenuScript))
                {
                    _decompressTvMenuScript = true;
                    _showHelp = false;
                }
                else if (arg.Trim().Equals(CommandLineArgumentGenerateChannelsFile))
                {
                    _generateChannelsFile = true;
                    _showHelp = false;
                }
                else if (arg.Trim().Equals(CommandLineArgumentDownloadChannelIcons))
                {
                    _downloadChannelIcons = true;
                    _showHelp = false;
                }
                else if (arg.Trim().Equals(CommandLineArgumentGenerateM3Ufile))
                {
                    _generateM3Ufile = true;
                    _showHelp = false;
                }
                else if (arg.Trim().Equals(CommandLineArgumentDownloadEpg))
                {
                    _downloadEpg = true;
                    _showHelp = false;
                }
                else if (arg.Trim().Equals(CommandLineArgumentDownloadDetails))
                {
                    _downloadDetails = true;
                }
                else if (arg.Trim().Equals(CommandLineArgumentDecompressEpg))
                {
                    _decompressEpg = true;
                    _showHelp = false;
                }
                else if (arg.Trim().Equals(CommandLineArgumentXmlTv))
                {
                    _xmlTv = true;
                    _showHelp = false;
                }
                else if (arg.Trim().Equals(CommandLineArgumentTVheadend))
                {
                    _generateTVheadend = true;
                    _showHelp = false;
                }
                else if (arg.Trim().Equals(CommandLineArgumentM3UtoTVheadend))
                {
                    _convertM3uToTVheadend = true;
                    _showHelp = false;
                }
                else if (arg.Trim().Equals(CommandLineArgumentAllM3U))
                {
                    _downloadTvMenu = true;
                    _decompressTvMenu = true;
                    _downloadTvMenuScript = true;
                    _decompressTvMenuScript = true;
                    _generateChannelsFile = true;
                    _generateM3Ufile = true;

                    _showHelp = false;
                }
                else if (arg.Trim().Equals(CommandLineArgumentAllXmlTv))
                {
                    _downloadEpg = true;
                    _decompressEpg = true;
                    _downloadDetails = true;
                    _xmlTv = true;

                    _showHelp = false;
                }
                else if (arg.Trim().Equals(CommandLineArgumentAll))
                {
                    _downloadTvMenu = true;
                    _decompressTvMenu = true;
                    _downloadTvMenuScript = true;
                    _decompressTvMenuScript = true;
                    _generateChannelsFile = true;
                    _generateM3Ufile = true;

                    _downloadEpg = true;
                    _decompressEpg = true;
                    _downloadDetails = true;
                    _xmlTv = true;

                    _generateTVheadend = true;

                    _showHelp = false;
                }
                else if (arg.Trim().Equals(CommandLineArgumentConvertM3U))
                {
                    _convertM3U = true;
                    _showHelp = false;
                }
                else if (arg.Trim().Equals(CommandLineArgumentIniSettings))
                {
                    _iniSettings = true;
                }
                else if (arg.Trim().Equals("/no-iptv"))
                {
                    _webDownloader = new NullWebDownloader();
                }
            }
        }

        private static ISettings LoadSettings()
        {
            return _iniSettings ? 
                LoadIniSettings() : 
                LoadConfigSettings();
        }

        private static ISettings LoadConfigSettings()
        {
            XmlConfigurator.Configure(new FileInfo("log4net.config"));
            return new ConfigSettings();
        }

        private static ISettings LoadIniSettings()
        {
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
