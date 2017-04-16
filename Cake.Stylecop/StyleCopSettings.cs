namespace Cake.Stylecop
{
    using Cake.Core.IO;

    /// <summary>
    /// Contains configuration for a stylecop analysis execution.
    /// </summary>
    public class StyleCopSettings 
    {
        /// <summary>
        /// Creates a new instance of the StyleCopSettings class.
        /// </summary>
        public StyleCopSettings()
        {
            Addins = new DirectoryPathCollection(new PathComparer(false));
            ProjectFiles = new FilePathCollection(new PathComparer(false));
        }

        /// <summary>
        /// The solution file
        /// </summary>
        public FilePath SolutionFile { get; set; }

        /// <summary>
        /// Indicates whether to write results cache files.
        /// </summary>
        public bool WriteResultsCache { get; set; }

        /// <summary>
        /// Determines whether to ignore cache files and reanalyze
        /// every file from scratch.
        /// </summary>
        public bool FullAnalyze { get; set; } = true;

        /// <summary>
        /// Indicates whether to load addins
        /// from the default path, where the core binary is located.
        /// </summary>
        public bool LoadFromDefaultPath { get; set; } = true;

        /// <summary>
        /// The path to the settings to load or
        /// null to use the default project settings files.
        /// </summary>
        public FilePath SettingsFile { get; set; }

        /// <summary>
        /// Optional path to the results output file.
        /// </summary>
        public FilePath ResultsFile { get; set; }

        /// <summary>
        /// The list of paths to search under for parser and analyzer addins.
        /// Can be null if no addin paths are provided.
        /// </summary>
        public DirectoryPathCollection Addins { get; }

        /// <summary>
        /// Outputs an html report using the default stylesheet or a custom one if specified in StyleSheet
        /// </summary>
        public FilePath HtmlReportFile { get; set; }

        /// <summary>
        /// The StyleSheet Path
        /// </summary>
        public FilePath StyleSheet { get; set; }

        /// <summary>
        /// The list of project paths to analyze
        /// </summary>
        public FilePathCollection ProjectFiles { get; set; }
    }
}