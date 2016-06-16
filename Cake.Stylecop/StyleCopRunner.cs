namespace Cake.Stylecop
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Xml.Xsl;

    using Cake.Common.IO;
    using Cake.Common.Solution;
    using Cake.Common.Solution.Project;
    using Cake.Core;
    using Cake.Core.IO;

    using StyleCop;

    public delegate StyleCopSettings SettingsDelegate(StyleCopSettings settings);

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
            Cake.Common.Diagnostics.LoggingAliases.Information(context, $"Stylecop: Default stylesheet {context.MakeAbsolute(defaultStyleSheet)}");

            var solutionFile = settings.SolutionFile;
            var settingsFile = settings.SettingsFile?.ToString();
            var outputPath = settings.ResultsFile?.ToString();
            var addins = settings.Addins.Count == 0 ? null : settings.Addins.Select(x => x.FullPath).ToList();
            
            var solutionParser = new SolutionParser(context.FileSystem, context.Environment);
            var projectParser = new ProjectParser(context.FileSystem, context.Environment);
        
            var projectPath = solutionFile.MakeAbsolute(context.Environment).GetDirectory();
        
            Cake.Common.Diagnostics.LoggingAliases.Information(context, $"Stylecop: Found solution {projectPath.FullPath}");
        
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
                Cake.Common.Diagnostics.LoggingAliases.Information(context, $"Stylecop: Found project {solutionProject.Path}");
                var project = projectParser.Parse(solutionProject.Path);
                var styleCopProject = new CodeProject(0, solutionProject.Path.GetDirectory().ToString(), new Configuration(null));
                styleCopProjects.Add(styleCopProject);
            
                foreach (var projectFile in project.Files)
                {
                    if (projectFile.FilePath.GetExtension() != ".cs") continue;

                    Cake.Common.Diagnostics.LoggingAliases.Debug(context, $"Stylecop: Found file {projectFile.FilePath}");
                    styleCopConsole.Core.Environment.AddSourceCode(styleCopProject, projectFile.FilePath.ToString(), null);
                }
            }                
                
            var handler = new StylecopHandlers(context);

            styleCopConsole.OutputGenerated += handler.OnOutputGenerated;
            styleCopConsole.ViolationEncountered += handler.ViolationEncountered;
            Cake.Common.Diagnostics.LoggingAliases.Information(context, $"Stylecop: Starting analysis");
            styleCopConsole.Start(styleCopProjects.ToArray(), settings.FullAnalyze);
            Cake.Common.Diagnostics.LoggingAliases.Information(context, $"Stylecop: Finished analysis");
            styleCopConsole.OutputGenerated -= handler.OnOutputGenerated;
            styleCopConsole.ViolationEncountered -= handler.ViolationEncountered;

            if (settings.HtmlReportFile != null)
            {
                // copy default resources to output folder
                context.CopyDirectory(context.Directory(toolPath + "/resources"), settings.HtmlReportFile.GetDirectory());

                Cake.Common.Diagnostics.LoggingAliases.Information(context, $"Stylecop: Creating html report {settings.HtmlReportFile.FullPath}");
                Transform(context, settings.ResultsFile.MakeAbsolute(context.Environment), settings.StyleSheet ?? context.MakeAbsolute(defaultStyleSheet));
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
        private static void Transform(ICakeContext context, FilePath outputXmlFile, FilePath transformFile)
        {
            if (!context.FileExists(outputXmlFile))
            {
                Cake.Common.Diagnostics.LoggingAliases.Warning(context, $"Stylecop: Output file not found {outputXmlFile.FullPath}");
                return;
            }

            if (!context.FileExists(transformFile))
            {
                Cake.Common.Diagnostics.LoggingAliases.Warning(context, $"Stylecop: Transform file not found {transformFile.FullPath}");
                return;
            }

            var xt = new XslCompiledTransform();
            Cake.Common.Diagnostics.LoggingAliases.Debug(context, $"Stylecop: Loading transform {transformFile.FullPath}");
            xt.Load(transformFile.FullPath);
            Cake.Common.Diagnostics.LoggingAliases.Debug(context, $"Stylecop: Loaded transform {transformFile.FullPath}");

            var htmlout = string.Format(CultureInfo.CurrentCulture, "{0}\\{1}.html", outputXmlFile.GetDirectory().FullPath, outputXmlFile.GetFilenameWithoutExtension());
            Cake.Common.Diagnostics.LoggingAliases.Debug(context, $"Stylecop: Starting transform {outputXmlFile.FullPath} to {htmlout}");
            xt.Transform(outputXmlFile.FullPath, htmlout);
            Cake.Common.Diagnostics.LoggingAliases.Debug(context, $"Stylecop: Finished transform {outputXmlFile.FullPath} to {htmlout}");
        }

        public static string AssemblyDirectory(Assembly assembly)
        {
            var codeBase = assembly.CodeBase;
            var uri = new UriBuilder(codeBase);
            return Uri.UnescapeDataString(uri.Path);
        }
    }    
}
