using TMPro;
using UnityEngine;

namespace SamsHelper.ReactiveUI.Elements
{
    public static class UiAppearanceController
    {
        public static readonly Color FadedColour = new Color(1f, 1f, 1f, 0.4f);
        public static readonly Color InvisibleColour = new Color(1f, 1f, 1f, 0f);
        private const int SmallFontSize = 10, MediumFontSize = 20, LargeFontSize = 30, TitleFontSize = 45;
        private static TMP_FontAsset UniversalFont;

        public static TMP_FontAsset GetFont()
        {
            if (UniversalFont == null) UniversalFont = Resources.Load<TMP_FontAsset>("ImFell SDF");
            return UniversalFont;
        }

        public static int GetFontSize(EnhancedText.FontSizes fontSizes)
        {
            switch (fontSizes)
            {
                case EnhancedText.FontSizes.Small:
                    return SmallFontSize;
                case EnhancedText.FontSizes.Medium:
                    return MediumFontSize;
                case EnhancedText.FontSizes.Large:
                    return LargeFontSize;
                case EnhancedText.FontSizes.Title:
                    return TitleFontSize;
            }

            return 0;
        }
    }
}