namespace Cake.Stylecop
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
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

    using StyleCop;

    public delegate StyleCopSettings SettingsDelegate(StyleCopSettings settings);
    public delegate StyleCopReportSettings ReportSettingsDelegate(StyleCopReportSettings settings);

    public static class StyleCopRunner
    {
        private static StyleCopSettings settings;

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
        
            var styleCopConsole = new StyleCopConsole(
                settingsFile, 
                settings.WriteResultsCache, /* Input Cache Result */ 
                outputPath, /* Output file */ 
                addins, 
                settings.LoadFromDefaultPath);
            
            var styleCopProjects = new List<CodeProject>();
        
            var solution = solutionParser.Parse(solutionFile);
            foreach (var solutionProject in solution.Projects)
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
                
            var handler = new StylecopHandlers(context);

            styleCopConsole.OutputGenerated += handler.OnOutputGenerated;
            styleCopConsole.ViolationEncountered += handler.ViolationEncountered;
            context.Log.Information($"Stylecop: Starting analysis");
            styleCopConsole.Start(styleCopProjects.ToArray(), settings.FullAnalyze);
            context.Log.Information($"Stylecop: Finished analysis");
            styleCopConsole.OutputGenerated -= handler.OnOutputGenerated;
            styleCopConsole.ViolationEncountered -= handler.ViolationEncountered;

            if (settings.HtmlReportFile != null)
            {
                // copy default resources to output folder
                context.CopyDirectory(context.Directory(toolPath + "/resources"), settings.HtmlReportFile.GetDirectory());

                context.Log.Information($"Stylecop: Creating html report {settings.HtmlReportFile.FullPath}");
                Transform(context, settings.HtmlReportFile, settings.ResultsFile.MakeAbsolute(context.Environment), settings.StyleSheet ?? context.MakeAbsolute(defaultStyleSheet));
            }

            if (handler.TotalViolations > 0)
            {
                throw new Exception($"{handler.TotalViolations} StyleCop violations encountered.");
            }
        }

        /// <summary>
        /// Transforms the outputted report using an XSL transform file.
        /// </summary>
        /// <param name="outputXmlFile">
        ///     The fully-qualified path of the report to transform.
        /// </param>
        /// <param name="transformFile">The filePath for the xslt transform</param>
        /// <param name="context"></param>
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

        public static string AssemblyDirectory(Assembly assembly)
        {
            var codeBase = assembly.CodeBase;
            var uri = new UriBuilder(codeBase);
            return Uri.UnescapeDataString(uri.Path);
        }

        public static void Report(ICakeContext context, ReportSettingsDelegate settingsDelegate)
        {
            try
            {
                var assemblyDirectory = AssemblyDirectory(Assembly.GetAssembly(typeof(StyleCopSettings)));
                var toolPath = context.File(assemblyDirectory).Path.GetDirectory();
                var defaultStyleSheet = context.File(toolPath + "/StyleCopStyleSheet.xslt");

                var settings = settingsDelegate(new StyleCopReportSettings());
                context.Log.Information($"StyleCopReportSetting.HtmlReport: {settings.HtmlReportFile}");
                context.Log.Information($"StyleCopReportSetting.ResultFiles: {settings.ResultFiles}");

                // merge xml files
                var resultFile = MergeResultFile(context, settings.ResultFiles);
                var mergedResultsFile = context.File(settings.HtmlReportFile.GetDirectory() + context.File("/stylecop_merged.xml"));
                context.Log.Information($"Stylecop: Saving merged results xml file {mergedResultsFile.Path.FullPath}");
                resultFile.Save(mergedResultsFile);

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
