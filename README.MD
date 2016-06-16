# Cake.StyleCop

This is a thin wrapper around the standard StyleCop libraries which exposes StyleCop in a manner which
can be used by Cake build scripts.

The .dlls included are available directly from [The official StyleCop Project page](https://stylecop.codeplex.com/SourceControl/latest#Tools/StyleCop/v4.7/)

```csharp
#addin "Cake.StyleCop"

var target = Argument("target", "StyleCop");

Task("StyleCop")
  .Does(() =>
{
    // solution file
    var solutionFile = File("MySolution.sln");

    // stylecop settings file
    var settingsFile = File("Settings.stylecop");
    
    // xml results file
    var resultFile = File("StylecopResults.xml");

    // html report file
    var htmlFile = File("StylecopResults.html");

    StyleCopAnalyse(settings => settings
        .WithSolution(solutionFile)
        .WithSettings(settingsFile)
        .ToResultFile(resultFile)
        .ToHtmlReport(htmlFile)
    );
});

RunTarget(target);
```