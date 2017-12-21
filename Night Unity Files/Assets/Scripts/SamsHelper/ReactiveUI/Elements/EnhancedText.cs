using SamsHelper.ReactiveUI.Elements;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.UI;

namespace Facilitating.UI.Elements
{
//    [ExecuteInEditMode]
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class EnhancedText : MonoBehaviour
    {
        private TextMeshProUGUI _text;
        public int CustomFontSize;

        public enum FontSizes
        {
            Small,
            Medium,
            Large,
            Title,
            Custom
        }

        public FontSizes FontSize;

        public void Awake()
        {
            TryReplaceText();
            _text.richText = true;
            UpdateFontSize();
            _text.extraPadding = true;
            _text.font = UiAppearanceController.Instance.UniversalFont;
        }

        public void Text(string text)
        {
            _text.text = text;
        }

        public void SetFont(TMP_FontAsset universalFont)
        {
            TryReplaceText();
            _text = gameObject.GetComponent<TextMeshProUGUI>();
            _text.font = universalFont;
            UpdateFontSize();
        }

        private void TryReplaceText()
        {
            if (GetComponent<TextMeshProUGUI>() == null)
            {
                Text t = gameObject.GetComponent<Text>();
                string textContent = t.text;
                DestroyImmediate(t);
                TextMeshProUGUI newtext = gameObject.AddComponent<TextMeshProUGUI>();
                newtext.text = textContent;
            }
            _text = gameObject.GetComponent<TextMeshProUGUI>();
            _text.spriteAsset = Resources.Load("Icons") as TMP_SpriteAsset;
        }
        
        public void Update()
        {
#if UNITY_EDITOR
            if (!EditorApplication.isPlayingOrWillChangePlaymode)
            {
                TryReplaceText();
                UpdateFontSize();
            }
#endif
        }

        private void UpdateFontSize()
        {
            TryReplaceText();
            if (FontSize != FontSizes.Custom)
            {
                _text.fontSize = UiAppearanceController.Instance.GetFontSize(FontSize);
            }
            else
            {
                _text.fontSize = CustomFontSize;
            }
        }

        public void SetColor(Color color)
        {
            TryReplaceText();
            _text.color = color;
        }
    }
}