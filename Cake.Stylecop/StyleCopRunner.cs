namespace Cake.Stylecop
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Xml.Linq;
    using System.Xml.Xsl;

    using Cake.Common.IO;
    using Cake.Common.Solution;
    using Cake.Common.Solution.Project;
    using Cake.Core;
    using Cake.Core.Diagnostics;
    using Cake.Core.IO;

    using global::StyleCop;
    
    /// <summary>
    /// A proxy onto the StyleCopSettings type.
    /// </summary>
    /// <param name="settings">The settings.</param>
    /// <returns>The settings.</returns>
    public delegate StyleCopSettings SettingsDelegate(StyleCopSettings settings);

    /// <summary>
    /// A proxy onto the StyleCopReportSettings type.
    /// </summary>
    /// <param name="settings">The settings.</param>
    /// <returns>The settings.</returns>
    public delegate StyleCopReportSettings ReportSettingsDelegate(StyleCopReportSettings settings);

    /// <summary>
    /// The class that executes stylecop analysis.
    /// </summary>
    public static class StyleCopRunner
    {
        private static StyleCopSettings settings;
        private const string FOLDER_PROJECT_TYPE_GUID = "{2150E333-8FDC-42A3-9474-1A3956D46DE8}";

        /// <summary>
        /// Starts an analysis run.
        /// </summary>
        /// <param name="context">The cake context.</param>
        /// <param name="settingsDelegate">The stylecop setting to use during the analysis.</param>
        public static void Execute(ICakeContext context, SettingsDelegate settingsDelegate)
        {
            settings = settingsDelegate(new StyleCopSettings());

            // need to get pwd for stylecop.dll for stylesheet
            var assemblyDirectory = AssemblyDirectory(Assembly.GetAssembly(typeof(StyleCopSettings)));
            var toolPath = context.File(assemblyDirectory).Path.GetDirectory();
            var defaultStyleSheet = context.File(toolPath + "/StyleCopStyleSheet.xslt");
            context.Log.Information($"Stylecop: Default stylesheet {context.MakeAbsolute(defaultStyleSheet)}");

            var solutionFile = settings.SolutionFile;
            var settingsFile = settings.SettingsFile?.ToString();
            var outputPath = settings.ResultsFile?.ToString();
            var addins = settings.Addins.Count == 0 ? null : settings.Addins.Select(x => x.FullPath).ToList();

            var solutionParser = new SolutionParser(context.FileSystem, context.Environment);
            var projectParser = new ProjectParser(context.FileSystem, context.Environment);

            var projectPath = solutionFile.MakeAbsolute(context.Environment).GetDirectory();

            context.Log.Information($"Stylecop: Found solution {projectPath.FullPath}");

            StyleCopConsole styleCopConsole = null;

            try
            {
                styleCopConsole = new StyleCopConsole(
                                      settingsFile,
                                      settings.WriteResultsCache,
                                      /* Input Cache Result */
                                      outputPath,
                                      /* Output file */
                                      addins,
                                      settings.LoadFromDefaultPath);
            }
            catch (TypeLoadException typeLoadException)
            {
                context.Log.Error($"Error: Stylecop was unable to load an Addin .dll. {typeLoadException.Message}");
                throw;
            }

            var styleCopProjects = new List<CodeProject>();
            
            var solution = solutionParser.Parse(solutionFile);
            foreach (var solutionProject in solution.Projects.Where(p => p.Type != FOLDER_PROJECT_TYPE_GUID))
            {
                context.Log.Information($"Stylecop: Found project {solutionProject.Path}");
                var project = projectParser.Parse(solutionProject.Path);
                var styleCopProject = new CodeProject(0, solutionProject.Path.GetDirectory().ToString(), new Configuration(null));
                styleCopProjects.Add(styleCopProject);

                foreach (var projectFile in project.Files)
                {
                    if (projectFile.FilePath.GetExtension() != ".cs") continue;

                    context.Log.Debug($"Stylecop: Found file {projectFile.FilePath}");
                    styleCopConsole.Core.Environment.AddSourceCode(styleCopProject, projectFile.FilePath.ToString(), null);
                }
            }

            var handler = new StylecopHandlers(context, settings);

            styleCopConsole.OutputGenerated += handler.OnOutputGenerated;
            styleCopConsole.ViolationEncountered += handler.ViolationEncountered;
            context.Log.Information($"Stylecop: Starting analysis");
            styleCopConsole.Start(styleCopProjects.ToArray(), settings.FullAnalyze);
            context.Log.Information($"Stylecop: Finished analysis");
            styleCopConsole.OutputGenerated -= handler.OnOutputGenerated;
            styleCopConsole.ViolationEncountered -= handler.ViolationEncountered;

            if (settings.HtmlReportFile != null)
            {
                settings.HtmlReportFile = settings.HtmlReportFile.MakeAbsolute(context.Environment);

                // copy default resources to output folder
                context.CopyDirectory(context.Directory(toolPath + "/resources"), settings.HtmlReportFile.GetDirectory() + "/resources");

                context.Log.Information($"Stylecop: Creating html report {settings.HtmlReportFile.FullPath}");
                Transform(context, settings.HtmlReportFile, settings.ResultsFile.MakeAbsolute(context.Environment), settings.StyleSheet ?? context.MakeAbsolute(defaultStyleSheet));
            }

            if (handler.TotalViolations > 0 && settings.FailTask)
            {
                throw new Exception($"{handler.TotalViolations} StyleCop violations encountered.");
            }
        }

        /// <summary>
        /// Transforms the outputted report using an XSL transform file.
        /// </summary>
        /// <param name="htmlFile">The fully qualified path of the output html file.</param>
        /// <param name="outputXmlFile">
        ///     The fully-qualified path of the report to transform.
        /// </param>
        /// <param name="transformFile">The filePath for the xslt transform</param>
        /// <param name="context">The cake context.</param>
        private static void Transform(ICakeContext context, FilePath htmlFile, FilePath outputXmlFile, FilePath transformFile)
        {
            if (!context.FileExists(outputXmlFile))
            {
                context.Log.Warning($"Stylecop: Output file not found {outputXmlFile.FullPath}");
                return;
            }

            if (!context.FileExists(transformFile))
            {
                context.Log.Warning($"Stylecop: Transform file not found {transformFile.FullPath}");
                return;
            }

            var xt = new XslCompiledTransform();
            context.Log.Debug($"Stylecop: Loading transform {transformFile.FullPath}");
            xt.Load(transformFile.FullPath);
            context.Log.Debug($"Stylecop: Loaded transform {transformFile.FullPath}");

            context.Log.Debug($"Stylecop: Starting transform {outputXmlFile.FullPath} to {htmlFile}");
            xt.Transform(outputXmlFile.FullPath, htmlFile.FullPath);
            context.Log.Debug($"Stylecop: Finished transform {outputXmlFile.FullPath} to {htmlFile}");
        }

        /// <summary>
        /// The Assembly Directory.
        /// </summary>
        /// <param name="assembly">Assembly to return the directory path for.</param>
        /// <returns>The assemblies directory path.</returns>
        public static string AssemblyDirectory(Assembly assembly)
        {
            var codeBase = assembly.CodeBase;
            var uri = new UriBuilder(codeBase);
            return Uri.UnescapeDataString(uri.Path);
        }

        /// <summary>
        /// Starts the report aggregation process.
        /// </summary>
        /// <param name="context">The cake context.</param>
        /// <param name="settingsDelegate">The settings to use during report aggregation.</param>
        public static void Report(ICakeContext context, ReportSettingsDelegate settingsDelegate)
        {
            try
            {
                var assemblyDirectory = AssemblyDirectory(Assembly.GetAssembly(typeof(StyleCopSettings)));
                var toolPath = context.File(assemblyDirectory).Path.GetDirectory();
                var defaultStyleSheet = context.File(toolPath + "/StyleCopStyleSheet.xslt");

                var settings = settingsDelegate(new StyleCopReportSettings());
                context.Log.Information($"StyleCopReportSetting.HtmlReport: {settings.HtmlReportFile}");

                if (settings.ResultFiles.Count > 1)
                {
                    context.Log.Information("StyleCopReportSetting.ResultFiles:");
                    foreach (var resultFile in settings.ResultFiles)
                    {
                        context.Log.Information($"    {resultFile}");
                    }
                }
                else
                {
                    context.Log.Information($"StyleCopReportSetting.ResultFiles: {settings.ResultFiles.First()}");
                }                

                // merge xml files
                var finalResultFile = MergeResultFile(context, settings.ResultFiles);
                var mergedResultsFile = context.File(settings.HtmlReportFile.GetDirectory() + context.File("/stylecop_merged.xml"));
                context.Log.Information($"Stylecop: Saving merged results xml file {mergedResultsFile.Path.FullPath}");

                if (!context.DirectoryExists(mergedResultsFile.Path.GetDirectory()))
                {
                    context.CreateDirectory(mergedResultsFile.Path.GetDirectory());
                }

                finalResultFile.Save(mergedResultsFile);

                // copy default resources to output folder
                context.CopyDirectory(context.Directory(toolPath + "/resources"), context.Directory(settings.HtmlReportFile.GetDirectory() + "/resources"));

                context.Log.Information($"Stylecop: Creating html report {settings.HtmlReportFile.FullPath}");
                Transform(context, settings.HtmlReportFile.MakeAbsolute(context.Environment), mergedResultsFile.Path.MakeAbsolute(context.Environment), settings.StyleSheet ?? context.MakeAbsolute(defaultStyleSheet));
            }
            catch (Exception e)
            {
                context.Log.Error(e.ToString());
            }
        }

        /// <summary>
        /// Merges two or more Stylecop report files into a single xml document.
        /// </summary>
        /// <param name="context">The cake context.</param>
        /// <param name="resultFiles">A collection of report files to merge.</param>
        /// <returns>The resultant Xml document.</returns>
        public static XDocument MergeResultFile(ICakeContext context, FilePathCollection resultFiles)
        {
            context.Log.Information($"Stylecop: Loading result xml file {resultFiles.First().FullPath}");
            var xFileRoot = XDocument.Load(resultFiles.First().FullPath);

            foreach (var resultFile in resultFiles.Skip(1))
            {
                context.Log.Information($"Stylecop: Loading result xml file {resultFile.FullPath}");
                var xFileChild = XDocument.Load(resultFile.FullPath);
                xFileRoot.Root.Add(xFileChild.Root.Elements());
            }

            return xFileRoot;
        }
    }
}
