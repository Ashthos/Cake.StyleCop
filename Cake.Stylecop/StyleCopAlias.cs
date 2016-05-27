﻿namespace Cake.Stylecop
{
    using System;
    using Cake.Core;
    using Cake.Core.Annotations;

    [CakeAliasCategory("StylecopCategory")]
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
            StyleCopRunner.Execute(
                context,
                settings => settings.WithSolution(null).WithSettings(null).ToResultFile(null));

        }
    }
}
