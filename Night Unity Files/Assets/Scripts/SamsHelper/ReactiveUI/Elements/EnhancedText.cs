using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace SamsHelper.ReactiveUI.Elements
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class EnhancedText : MonoBehaviour
    {
        public enum FontSizes
        {
            Small,
            Medium,
            Large,
            Title,
            Custom
        }

        private TextMeshProUGUI _text;
        public float CustomFontSize;

        public FontSizes FontSize;
        private bool _strikethroughActive;

        public void Awake()
        {
            _text = GetComponent<TextMeshProUGUI>();
            _text.richText = true;
            _text.extraPadding = true;
            _text.font = UiAppearanceController.GetFont();
            TryReplaceText();
            UpdateFontSize();
        }

        public void SetText(string text)
        {
            if (_text == null) _text = GetComponent<TextMeshProUGUI>();
            _text.text = text;
            UpdateStrikeThrough();
        }

        public void SetStrikeThroughActive(bool active)
        {
            _strikethroughActive = active;
            UpdateStrikeThrough();
        }

        private void UpdateStrikeThrough()
        {
            if (_strikethroughActive)
            {
                string text = "<s>" + _text.text + "</s>";
                _text.color = UiAppearanceController.FadedColour;
                _text.text = text;
            }
            else
            {
                string text = _text.text.Replace("<s>", "");
                text = text.Replace("</s>", "");
                _text.text = text;
                _text.color = Color.white;
            }
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
                _text.fontSize = UiAppearanceController.GetFontSize(FontSize);
            else
                _text.fontSize = CustomFontSize;
        }

        public void SetColor(Color color)
        {
            TryReplaceText();
            _text.color = color;
        }
    }
}