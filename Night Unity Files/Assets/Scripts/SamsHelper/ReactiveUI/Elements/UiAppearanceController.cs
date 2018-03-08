using System.Collections.Generic;
using Facilitating.UI.Elements;
using TMPro;
using UnityEngine;

namespace SamsHelper.ReactiveUI.Elements
{
//    [ExecuteInEditMode]
    public class UiAppearanceController : MonoBehaviour
    {
        public TMP_FontAsset UniversalFont;
        public int SmallFontSize = 10, MediumFontSize = 15, LargeFontSize = 30, TitleFontSize = 45;
        private static UiAppearanceController _instance;
        public static readonly Color FadedColour = new Color(1f, 1f, 1f, 0.4f);
        public static readonly Color InvisibleColour = new Color(1f, 1f, 1f, 0f);


        public void Awake()
        {
            _instance = this;
        }

        public static UiAppearanceController Instance()
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<UiAppearanceController>();
            }
            return _instance;
        }

        public void UpdateTextFont()
        {
            List<EnhancedText> _texts = Helper.FindAllComponentsInChildren<EnhancedText>(transform);
            _texts.ForEach(t => t.SetFont(UniversalFont));
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

        public int GetFontSize(EnhancedText.FontSizes fontSizes)
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