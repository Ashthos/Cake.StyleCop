// To create the bootstrapper, from within powershell run the following.
// Invoke-WebRequest http://cakebuild.net/bootstrapper/windows -OutFile build.ps1
//
// To execute, run the following within powershell
// ./Build.ps1 -Target "build"

#Addin "Cake.StyleCop"

const string Configuration = "Release";

    var target = Argument("target", "Build");
    var nugetApiKey = Argument<string>("nugetApi", null);
    var nugetSource = Argument<string>("nugetSource", null); // nuget.org

    var solutionFile = File("./Cake.StyleCop.sln");
    var artifactsDir = Directory("./artifacts");
    var nupkgDestDir = artifactsDir + Directory("nuget-package");
    var stylecopResultsDir = artifactsDir + Directory("stylecop");
    var stylecopReportsDir = stylecopResultsDir + Directory("stylecop-reports");


    Task("Clean")
    .Does(() => {
        CleanDirectories(new DirectoryPath[] {
            //artifactsDir,
            nupkgDestDir,
            stylecopResultsDir,
            stylecopReportsDir
        });

        CleanDirectories("./**/bin/**");
    });

    Task("Build")
    .IsDependentOn("Clean")
    .Does(() => {

        Information("Restoring Nuget Packages");
        NuGetRestore(solutionFile);
            
        var settings = new MSBuildSettings();
        settings.Configuration = Configuration;
        settings.WithTarget("build");
            
        Information("Compiling Solution");
        MSBuild(solutionFile, settings);

    });
    
    Task("Code-Quality")
        .IsDependentOn("Build")
        .ContinueOnError()
        .Does(() => {
        
            var settingsFile = solutionFile.Path.GetDirectory() + File("Settings.stylecop");
            var resultFile = stylecopResultsDir + File("StylecopResults.xml");
            var htmlFile = stylecopReportsDir + File("StylecopResults.html");

            StyleCopAnalyse(settings => settings
                .WithSolution(solutionFile)
                .WithSettings(settingsFile)
                .ToResultFile(resultFile)
            );

            var resultFilePattern = stylecopResultsDir.Path + "/*.xml";
            Information("resultFilePattern: {0}", resultFilePattern);
            var resultFiles = GetFiles(resultFilePattern);
            foreach(var file in resultFiles){
                Information("resultFile: {0}", file.FullPath);
            }
            
            StyleCopReport(settings => settings
                .ToHtmlReport(htmlFile)
                .AddResultFiles(resultFiles)
            );
        });

    Task("Package")
    .IsDependentOn("Code-Quality")
    .Does(() => {
	
    var nuGetPackSettings   = new NuGetPackSettings {
    Version                 = "1.2.8",
    BasePath                = "./Cake.StyleCop",
    OutputDirectory         = nupkgDestDir
    };

    NuGetPack(File("./Cake.StyleCop/Cake.StyleCop.nuspec"), nuGetPackSettings);

    });
    
    Task("Publish")
        .IsDependentOn("Package")
        .Does(() => {
        
            var packages = GetFiles("./nuget/Cake.StyleCop.*.nupkg");
                
            foreach (var package in packages) {    
                // Push the package.
                NuGetPush(package, new NuGetPushSettings {
                    Source = nugetSource,
                    ApiKey = nugetApiKey
                    });
            }
    });

    RunTarget(target);