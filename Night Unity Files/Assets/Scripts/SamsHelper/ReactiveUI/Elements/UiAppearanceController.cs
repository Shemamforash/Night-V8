using System.Collections.Generic;
using SamsHelper.Libraries;
using TMPro;
using UnityEngine;

namespace SamsHelper.ReactiveUI.Elements
{
//    [ExecuteInEditMode]
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

        public static void UpdateTextFont()
        {
            List<EnhancedText> _texts = Helper.FindAllComponentsInChildren<EnhancedText>(GameObject.Find("Canvas").transform);
            _texts.ForEach(t => t.SetFont(GetFont()));
        }

//#if UNITY_EDITOR
//        public void Update()
//        {
//            UpdateTextFont();
//        }
//#endif

//        public void SetFontSize(EnhancedText.FontSizes fontSize, int newSize)
//        {
//            switch (fontSize)
//            {
//                case EnhancedText.FontSizes.Small:
//                    Debug.Log("banana");
//                    _smallFontSize = newSize;
//                    break;
//                case EnhancedText.FontSizes.Medium:
//                    _mediumFontSize = newSize;
//                    break;
//                case EnhancedText.FontSizes.Large:
//                    _largeFontSize = newSize;
//                    break;
//                case EnhancedText.FontSizes.Title:
//                    _titleFontSize = newSize;
//                    break;
//            }
//            UpdateTextFont();
//        }

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