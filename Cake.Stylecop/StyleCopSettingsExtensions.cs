namespace Cake.Stylecop
{
    using System;

    using Cake.Core.IO;

    public static class StyleCopSettingsExtensions
    {
        /// <summary>
        /// Specifies the .net solution to analyse.
        /// </summary>
        /// <param name="settings">The settings object.</param>
        /// <param name="solutionFile">FilePath of the .sln file.</param>
        /// <returns>Settings object.</returns>
        public static StyleCopSettings WithSolution(this StyleCopSettings settings, FilePath solutionFile)
        {
            if (solutionFile == null)
            {
                throw new ArgumentNullException(nameof(solutionFile), "Solution file path is null.");
            }

            settings.SolutionFile = solutionFile;
            return settings;
        }

        /// <summary>
        /// Specifies the Stylecop.settings file path.
        /// </summary>
        /// <param name="settings">The settings object.</param>
        /// <param name="settingsFile">FilePath of the Stylecop.settings file.</param>
        /// <returns>Settings object.</returns>
        public static StyleCopSettings WithSettings(this StyleCopSettings settings, FilePath settingsFile)
        {
            settings.SettingsFile = settingsFile;
            return settings;
        }

        /// <summary>
        /// Specifies which directories to load Stylecop addins from. 
        /// If none are specified and LoadFromDefaultPath is true all .dll files next to the stylecop.dll file will be added as potential addins.
        /// </summary>
        /// <param name="settings">The settings object.</param>
        /// <param name="addins">Directory paths for Stylecop Addins.</param>
        /// <returns>Settings object;</returns>
        public static StyleCopSettings WithAddins(this StyleCopSettings settings, params DirectoryPath[] addins)
        {
            foreach (var filePath in addins)
            {
                settings.Addins.Add(filePath);
            }

            return settings;
        }

        /// <summary>
        /// Use the stylecop result cache to speed up analysis.
        /// </summary>
        /// <param name="settings">The settings object.</param>
        /// <param name="enabled">True (default) to use cache, otherwise false.</param>
        /// <param name="fullAnalyze">Perform full analysis.</param>
        /// <returns>Settings object.</returns>
        public static StyleCopSettings UsingResultCache(this StyleCopSettings settings, bool enabled = true, bool fullAnalyze = false)
        {
            settings.WriteResultsCache = enabled;
            settings.FullAnalyze = fullAnalyze;
            return settings;
        }

        /// <summary>
        /// Indicates if Stylecop should load addins from the same directory as stylecop.dll.
        /// </summary>
        /// <param name="settings">The settings object.</param>
        /// <param name="enabled">True (default) to load .dll next to stylecop.dll, otherwise false.</param>
        /// <returns>Settings object.</returns>
        public static StyleCopSettings LoadFromDefaultPath(this StyleCopSettings settings, bool enabled = true)
        {
            settings.LoadFromDefaultPath = enabled;
            return settings;
        }

        /// <summary>
        /// Indicates the filepath to output the stylecop results to.
        /// </summary>
        /// <param name="settings">The settings object.</param>
        /// <param name="resultFile">The output filepath.</param>
        /// <returns>Settings object.</returns>
        public static StyleCopSettings ToResultFile(this StyleCopSettings settings, FilePath resultFile)
        {
            settings.ResultsFile = resultFile;
            return settings;
        }

        /// <summary>
        /// Indicates the results should be outputted as an HTML report.
        /// </summary>
        /// <param name="settings">The settings object.</param>
        /// <param name="htmlFile">The filepath for the html report.</param>
        /// <param name="xsltStylesheet">(Optional) The filepath for the xslt stylesheet. If omitted the default supplied with Cake.Stylecop is used.</param>
        /// <returns>Settings object.</returns>
        public static StyleCopSettings ToHtmlReport(this StyleCopSettings settings, FilePath htmlFile, FilePath xsltStylesheet = null)
        {
            settings.HtmlReportFile = htmlFile;
            settings.StyleSheet = xsltStylesheet;
            return settings;
        }

        /// <summary>
        /// Indicates the results should be outputted as an HTML report.
        /// </summary>
        /// <param name="settings">The settings object.</param>
        /// <param name="htmlFile">The filepath for the html report.</param>
        /// <param name="xsltStylesheet">(Optional) The filepath for the xslt stylesheet. If omitted the default supplied with Cake.Stylecop is used.</param>
        /// <returns>Settings object.</returns>
        public static StyleCopReportSettings ToHtmlReport(this StyleCopReportSettings settings, FilePath htmlFile, FilePath xsltStylesheet = null)
        {
            settings.HtmlReportFile = htmlFile;
            settings.StyleSheet = xsltStylesheet;
            return settings;
        }

        /// <summary>
        /// Allows multiple results files to be aggregated into a single report output.
        /// </summary>
        /// <param name="settings">The settings object.</param>
        /// <param name="resultFiles">The report files to aggregate.</param>
        /// <returns>Settings object.</returns>
        public static StyleCopReportSettings AddResultFiles(this StyleCopReportSettings settings, FilePathCollection resultFiles)
        {
            settings.ResultFiles = resultFiles;
            return settings;
        }
    }
}