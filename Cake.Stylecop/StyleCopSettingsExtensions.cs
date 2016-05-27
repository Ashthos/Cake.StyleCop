namespace Cake.Stylecop
{
    using System;

    using Cake.Core.IO;

    public static class StyleCopSettingsExtensions
    {
        public static StyleCopSettings WithSolution(this StyleCopSettings settings, FilePath solutionFile)
        {
            if (solutionFile == null)
            {
                throw new ArgumentNullException(nameof(solutionFile), "Solution file path is null.");
            }
            settings.SolutionFile = solutionFile;
            return settings;
        }

        public static StyleCopSettings WithSettings(this StyleCopSettings settings, FilePath settingsFile)
        {
            settings.SettingsFile = settingsFile;
            return settings;
        }

        public static StyleCopSettings WithAddins(this StyleCopSettings settings, params DirectoryPath[] addins)
        {
            foreach (var filePath in addins)
            {
                settings.Addins.Add(filePath);
            }
            return settings;
        }

        public static StyleCopSettings UsingResultCache(this StyleCopSettings settings, bool enabled = true, bool fullAnalyze = false)
        {
            settings.WriteResultsCache = enabled;
            settings.FullAnalyze = fullAnalyze;
            return settings;
        }

        public static StyleCopSettings LoadFromDefaultPath(this StyleCopSettings settings, bool enabled = true)
        {
            settings.LoadFromDefaultPath = enabled;
            return settings;
        }

        public static StyleCopSettings ToResultFile(this StyleCopSettings settings, FilePath resultFile)
        {
            settings.ResultsFile = resultFile;
            return settings;
        }
    }
}