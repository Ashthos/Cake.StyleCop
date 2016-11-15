namespace Cake.Stylecop
{
    using System;

    using Cake.Core;
    using Cake.Core.Annotations;

    /// <summary>
    /// Contains functionality for working with Stylecop.
    /// </summary>
    [CakeAliasCategory("Stylecop")]
    public static class StylecopAlias
    {
        /// <summary>
        /// Analyses the project using stylecop.
        /// </summary>
        /// <example>
        /// StyleCopAnalyse(settings => settings
        ///     .WithSolution(solutionFile)       
        /// );
        /// </example>
        /// <param name="context">The Context.</param>
        /// <param name="settingsDelegate">Optional settings passed to stylecop.</param>
        [CakeMethodAlias]
        public static void StyleCopAnalyse(this ICakeContext context, SettingsDelegate settingsDelegate)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            
            if (settingsDelegate == null)
            {
                throw new ArgumentNullException(nameof(settingsDelegate));
            }
            
            StyleCopRunner.Execute(context, settingsDelegate);
        }

        /// <summary>
        /// Generates summary report from a stylecop execution.
        /// </summary>
        /// <example>
        /// StyleCopAnalyse(settings => settings
        ///     .WithSolution(solutionFile)
        ///     .WithSettings(settingsFile)
        ///     .ToResultFile(resultFile)
        /// );
        /// 
        /// StyleCopReport(settings => settings
        ///     .ToHtmlReport(htmlFile)
        ///     .AddResultFiles(resultFiles)
        /// ); 
        /// </example>
        /// <param name="context">The context.</param>
        /// <param name="settingsDelegate">Report generation settings.</param>
        [CakeMethodAlias]
        public static void StyleCopReport(this ICakeContext context, ReportSettingsDelegate settingsDelegate)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (settingsDelegate == null)
            {
                throw new ArgumentNullException(nameof(settingsDelegate));
            }
            
            StyleCopRunner.Report(context, settingsDelegate);
        }
    }
}
