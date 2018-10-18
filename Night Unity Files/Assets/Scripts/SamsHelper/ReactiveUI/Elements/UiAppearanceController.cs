using TMPro;
using UnityEngine;

namespace SamsHelper.ReactiveUI.Elements
{
    public static class UiAppearanceController
    {
        public static readonly Color FadedColour = new Color(1f, 1f, 1f, 0.4f);
        public static readonly Color InvisibleColour = new Color(1f, 1f, 1f, 0f);
        private const int SmallFontSize = 10, MediumFontSize = 20, LargeFontSize = 30, TitleFontSize = 45;
        private static TMP_FontAsset UppercaseFont;
        private static TMP_FontAsset LowercaseFont;

        public static TMP_FontAsset GetFont()
        {
            if (UppercaseFont == null) UppercaseFont = Resources.Load<TMP_FontAsset>("ImFell Upper");
            if (LowercaseFont == null) LowercaseFont = Resources.Load<TMP_FontAsset>("ImFell Lower");
            return LowercaseFont;
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