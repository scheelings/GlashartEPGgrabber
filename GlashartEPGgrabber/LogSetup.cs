using System;
using System.Linq;
using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;

namespace GlashartEPGgrabber
{
    public static class LogSetup
    {
        public static void Setup()
        {
            var hierarchy = (Hierarchy)LogManager.GetRepository();
            hierarchy.Clear();

            var patternLayout = new PatternLayout
            {
                ConversionPattern = "%date %-5level %logger - %message%newline"
            };
            patternLayout.ActivateOptions();
            var consoleLayout = new PatternLayout
            {
                ConversionPattern = "%message%newline"
            };
            patternLayout.ActivateOptions();

            var roller = new RollingFileAppender
            {
                AppendToFile = true,
                File = @"GlashartEpg.log",
                Layout = patternLayout,
                MaxSizeRollBackups = 5,
                MaximumFileSize = "100MB",
                RollingStyle = RollingFileAppender.RollingMode.Size,
                StaticLogFileName = true
            };
            roller.ActivateOptions();
            hierarchy.Root.AddAppender(roller);

            var console = new ConsoleAppender
            {
                Layout = consoleLayout,
                Name = "Console",
                Target = "Console.Out",
            };
            console.ActivateOptions();
            hierarchy.Root.AddAppender(console);

            hierarchy.Root.Level = Level.Info;
#if DEBUG
            hierarchy.Root.Level = Level.Debug;
#endif
            hierarchy.Configured = true;
        }

        public static void ChangeLogLevel(string level)
        {
            var hierarchy = (Hierarchy)LogManager.GetRepository();
            var loglevel = hierarchy
                .LevelMap
                .AllLevels
                .Cast<Level>()
                .FirstOrDefault(l => String.Compare(l.Name, level, StringComparison.OrdinalIgnoreCase) == 0);
            hierarchy.Root.Level = loglevel ?? Level.Info;
            hierarchy.RaiseConfigurationChanged(EventArgs.Empty);
        }
    }
}