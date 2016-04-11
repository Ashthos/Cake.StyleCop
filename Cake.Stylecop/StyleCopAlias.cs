namespace Cake.Stylecop
{
    using System;
    using Cake.Core;
    using Cake.Core.Annotations;
    using Cake.Core.IO;

    [CakeAliasCategory("StylecopCategory")]
    public static class StylecopAlias
    {
        [CakeMethodAlias]
        public static void StyleCopAnalyse(this ICakeContext context, FilePath solutionFile, FilePath settingsFile)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            
            if (solutionFile == null)
            {
                throw new ArgumentNullException("solutionFile");
            }

            StyleCopRunner.Execute(context, solutionFile, settingsFile);
        }
    }
}
