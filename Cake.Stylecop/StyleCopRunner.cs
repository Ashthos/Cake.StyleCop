namespace Cake.Stylecop
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Cake.Common.Solution;
    using Cake.Common.Solution.Project;
    using Cake.Core;
    using StyleCop;

    public delegate StyleCopSettings SettingsDelegate(StyleCopSettings settings);

    public static class StyleCopRunner
    {
        private static StyleCopSettings settings;

        public static void Execute(ICakeContext context, SettingsDelegate settingsDelegate)
        {
            settings = settingsDelegate(new StyleCopSettings());
            
            var solutionFile = settings.SolutionFile;
            var settingsFile = settings.SettingsFile?.ToString();
            var outputPath = settings.ResultsFile?.ToString();
            var addins = settings.Addins.Count == 0 ? null : settings.Addins.Select(x => x.FullPath).ToList();
            
            var solutionParser = new SolutionParser(context.FileSystem, context.Environment);
            var projectParser = new ProjectParser(context.FileSystem, context.Environment);
        
            var projectPath = Cake.Common.IO.DirectoryAliases.Directory(context, solutionFile.MakeAbsolute(context.Environment).GetDirectory().FullPath);
        
            Cake.Common.Diagnostics.LoggingAliases.Information(context, string.Format("Project Path: {0}", projectPath.Path.FullPath));
        
            var styleCopConsole = new StyleCop.StyleCopConsole(
                settingsFile, 
                settings.WriteResultsCache, /* Input Cache Result */ 
                outputPath, /* Output file */ 
                addins, 
                settings.LoadFromDefaultPath);
            
            var styleCopProjects = new List<CodeProject>();
        
            var solution = solutionParser.Parse(solutionFile);
            foreach (var solutionProject in solution.Projects)
            {
                var project = projectParser.Parse(solutionProject.Path);
                var styleCopProject = new CodeProject(0, solutionProject.Path.GetDirectory().ToString(), new Configuration(null));
                styleCopProjects.Add(styleCopProject);
            
                foreach (var projectFile in project.Files)
                {
                    if (projectFile.FilePath.GetExtension() == ".cs")
                    {
                        styleCopConsole.Core.Environment.AddSourceCode(
                            styleCopProject, 
                            projectFile.FilePath.ToString(), 
                            null); 
                    }               
                }
            }                
                
            var handler = new StylecopHandlers(context);

            styleCopConsole.OutputGenerated += handler.OnOutputGenerated;
            styleCopConsole.ViolationEncountered += handler.ViolationEncountered;
            styleCopConsole.Start(styleCopProjects.ToArray(), settings.FullAnalyze);
            styleCopConsole.OutputGenerated -= handler.OnOutputGenerated;
            styleCopConsole.ViolationEncountered -= handler.ViolationEncountered;

            if (handler.TotalViolations > 0)
            {
                throw new Exception(string.Format("{0} StyleCop violations encountered.", handler.TotalViolations));
            }
        }        
    }    
}
