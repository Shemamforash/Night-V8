using System.Collections.Generic;
using SamsHelper;
using UI.Misc.Elements;
using UnityEngine;
using UnityEngine.UI;

namespace Facilitating.UI.Elements
{
    public class UiAppearanceController : MonoBehaviour
    {
        [HideInInspector]
        public Font UniversalFont;
        public Color MainColor, SecondaryColor;
        public Image BorderImage;
        public int _smallFontSize, _mediumFontSize, _largeFontSize, _titleFontSize;
        private static UiAppearanceController _instance;

        [ExecuteInEditMode]
        public void Awake()
        {
            _instance = this;
        }

        public static UiAppearanceController Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<UiAppearanceController>();
                }
                return _instance;
            }
        }

        public void UpdateTextFont()
        {
            List<EnhancedText> _texts = Helper.FindAllComponentsInChildren<EnhancedText>(transform);
            _texts.ForEach(t => t.SetFont(UniversalFont));
        }

        [ExecuteInEditMode]
        public void Update()
        {
            UpdateTextFont();
        }

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
                    return _smallFontSize;
                case EnhancedText.FontSizes.Medium:
                    return _mediumFontSize;
                case EnhancedText.FontSizes.Large:
                    return _largeFontSize;
                case EnhancedText.FontSizes.Title:
                    return _titleFontSize;
            }
            return 0;
        }
    }
}