namespace Cake.Stylecop
{
    using System;
    using System.Collections.Generic;
    using Cake.Common.Solution;
    using Cake.Common.Solution.Project;
    using Cake.Core;
    using Cake.Core.IO;
    using StyleCop;

    public static class StyleCopRunner
    {
        public static void Execute(ICakeContext context, FilePath solutionFile, FilePath settingsFile)
        {
            if (solutionFile == null)
            {
                throw new ArgumentNullException(nameof(solutionFile), "Solution file path is null.");
            }                       

            var solutionParser = new SolutionParser(context.FileSystem, context.Environment);
            var projectParser = new ProjectParser(context.FileSystem, context.Environment);
        
            string stylecopSettingsFile = settingsFile == null ? null : settingsFile.ToString();
            var projectPath = Cake.Common.IO.DirectoryAliases.Directory(context, solutionFile.Path.GetDirectory());
        
            Cake.Common.Diagnostics.LoggingAliases.Information(context, $"Project Path: {projectPath.Path.FullPath}");
        
            var styleCopConsole = new StyleCop.StyleCopConsole(
                stylecopSettingsFile, 
                false, /* Input Cache Result */ 
                null, /* Output file */ 
                null, 
                true);
            
            var scProjects = new List<CodeProject>();
        
            var solution = solutionParser.Parse(solutionFile);
            foreach(var solutionProject in solution.Projects){
                var project = projectParser.Parse(solutionProject.Path);
                var styleCopProject = new CodeProject(0, solutionProject.Path.GetDirectory().ToString(), new Configuration(null));
                scProjects.Add(styleCopProject);
            
                foreach(var projectFile in project.Files){
                                            
                    if (projectFile.FilePath.GetExtension() == ".cs"){
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
            styleCopConsole.Start(scProjects.ToArray(), true);
            styleCopConsole.OutputGenerated -= handler.OnOutputGenerated;
            styleCopConsole.ViolationEncountered -= handler.ViolationEncountered;

            if (handler.TotalViolations > 0){
                throw new Exception($"{handler.TotalViolations} StyleCop violations encountered.");
            }
        }

        
    }

    
}
