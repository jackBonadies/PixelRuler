using System;
using System.Windows;
using System.Windows.Markup;
using Wpf.Ui.Appearance;

namespace PixelRuler
{
    /// <summary>
    /// Wpf.UI ThemesDictionary but for my resource dictionaries
    /// </summary>
    [Localizability(LocalizationCategory.Ignore)]
    [Ambient]
    [UsableDuringInitialization(true)]
    public class ThemesDictionary : ResourceDictionary
    {
        /// <summary>
        /// Sets the default application theme.
        /// </summary>
        public ApplicationTheme Theme
        {
            set => SetSourceBasedOnSelectedTheme(value);
        }

        public ThemesDictionary()
        {
            SetSourceBasedOnSelectedTheme(ApplicationTheme.Light);
        }

        private void SetSourceBasedOnSelectedTheme(ApplicationTheme? selectedApplicationTheme)
        {
            var themeName = selectedApplicationTheme switch
            {
                ApplicationTheme.Dark => "Dark",
                ApplicationTheme.HighContrast => "HighContrast",
                _ => "Light"
            };

            Source = new Uri($"Resources/{themeName}.xaml", UriKind.Relative);
        }
    }
}
