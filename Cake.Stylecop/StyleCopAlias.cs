namespace Cake.Stylecop
{
    using System;

    using Cake.Core;
    using Cake.Core.Annotations;

    [CakeAliasCategory("Stylecop")]
    public static class StylecopAlias 
    {
        [CakeMethodAlias]
        public static void StyleCopAnalyse(this ICakeContext context, SettingsDelegate settingsDelegate)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            
            if (settingsDelegate == null)
            {
                throw new ArgumentNullException(nameof(settingsDelegate));
            }
            
            StyleCopRunner.Execute(context, settingsDelegate);
        }

        [CakeMethodAlias]
        public static void StyleCopReport(this ICakeContext context, ReportSettingsDelegate settingsDelegate)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (settingsDelegate == null)
            {
                throw new ArgumentNullException(nameof(settingsDelegate));
            }
            
            StyleCopRunner.Report(context, settingsDelegate);
        }
    }
}
