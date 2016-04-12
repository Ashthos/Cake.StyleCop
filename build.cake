// To create the bootstrapper, from within powershell run the following.
// Invoke-WebRequest http://cakebuild.net/bootstrapper/windows -OutFile build.ps1
//
// To execute, run the following within powershell
// ./Build.ps1 -Target "build"

#Addin "Cake.StyleCop"

const string Configuration = "Release";

var target = Argument("target", "Build");

Task("Clean")
	.Does(() => {
		CleanDirectories("./**/bin/**");
	});

Task("Build")
	.IsDependentOn("Clean")
	.Does(() => {

		var solutionFile = new FilePath("Cake.StyleCop.sln");
        
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
    .Does(() => {
        var solutionFile = File("./Cake.StyleCop.sln");
        StyleCopAnalyse(solutionFile, null);
    });

Task("Package")
    .IsDependentOn("Code-Quality")
	.Does(() => {
	
        if (!DirectoryExists("./nuget")){
            CreateDirectory("./nuget");
        }
    
		var nuGetPackSettings   = new NuGetPackSettings {
                Version                 = "1.0.0",
                BasePath                = "./Cake.StyleCop",
                OutputDirectory         = "./nuget"
            };

		NuGetPack(File("./Cake.StyleCop/Cake.StyleCop.nuspec"), nuGetPackSettings);

	});
    
Task("Publish")
    .IsDependentOn("Package")
    .Does(() => {
       
        var nugetApiKey = EnvironmentVariable("NugetApiKey");
       
        var package = "./nuget/Cake.StyleCop.1.0.0.nupkg";
            
        // Push the package.
        NuGetPush(package, new NuGetPushSettings {
            Source = null, // nuget.org
            ApiKey = nugetApiKey
        });
        
    });

RunTarget(target);