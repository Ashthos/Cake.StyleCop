namespace Cake.Stylecop
{
    using Cake.Core.IO;

    public class StyleCopReportSettings
    {
        public StyleCopReportSettings()
        {
            ResultFiles = new FilePathCollection(new PathComparer(false));
        }

        /// <summary>
        /// A collection of xml result files to merge for the report
        /// </summary>
        public FilePathCollection ResultFiles { get; set; }

        /// <summary>
        /// Outputs an html report using the default stylesheet or a custom one if specified in StyleSheet
        /// </summary>
        public FilePath HtmlReportFile { get; set; }

        /// <summary>
        /// The StyleSheet Path
        /// </summary>
        public FilePath StyleSheet { get; set; }
    }
}