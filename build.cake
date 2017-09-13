// To create the bootstrapper, from within powershell run the following.
// Invoke-WebRequest http://cakebuild.net/bootstrapper/windows -OutFile build.ps1
//
// To execute, run the following within powershell
// ./Build.ps1 -Target "build"

// When debugging, use instead of the #Addin above.
// Note: VS Build often leaves additional dlls in the /bin/* directory that Stylecop errors when attempting to load. Delete these as necessary.
// #r "Cake.Stylecop/bin/Release/Cake.StyleCop.dll"

const string Configuration = "Release";

    var target = Argument("target", "Build");
    var nugetApiKey = Argument<string>("nugetApi", EnvironmentVariable("NugetApiKey"));
    var nugetSource = Argument<string>("nugetSource", "https://www.nuget.org/api/v2/package");

    var solutionFile = File("./Cake.StyleCop.sln");
    var artifactsDir = Directory("./artifacts");
    var nupkgDestDir = artifactsDir + Directory("nuget-package");
    var stylecopResultsDir = artifactsDir + Directory("stylecop");
    var stylecopReportsDir = stylecopResultsDir + Directory("stylecop-reports");


    Task("Clean")
    .Does(() => {
        CleanDirectories(new DirectoryPath[] {
            artifactsDir,
            nupkgDestDir,
            stylecopResultsDir,
            stylecopReportsDir
        });

		try {
			CleanDirectories("./**/bin/**");
		} catch (Exception exception) {
			Warning("Failed to clean one or more directories.");
		}

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
    
    Task("Package")
    .IsDependentOn("Build")
    .Does(() => {
	
		var nuGetPackSettings   = new NuGetPackSettings {
		Version                 = "1.1.4",
		BasePath                = "./Cake.StyleCop",
		OutputDirectory         = nupkgDestDir
		};

		NuGetPack(File("./Cake.StyleCop/Cake.StyleCop.nuspec"), nuGetPackSettings);

    });
    
    Task("Publish")
        .IsDependentOn("Package")
        .Does(() => {
        
            var packages = GetFiles(nupkgDestDir.Path + "/Cake.StyleCop.*.nupkg");
                
            foreach (var package in packages) {    
                // Push the package.
                NuGetPush(package, new NuGetPushSettings {
                    Source = nugetSource,
                    ApiKey = nugetApiKey
                    });
            }
    });

    RunTarget(target);
