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

        public enum FontSizes
        {
            Small,
            Medium,
            Large,
            Title
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
            _text.spriteAsset = Resources.Load("Sprites/Icons") as TMP_SpriteAsset;
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
            _text.fontSize = UiAppearanceController.Instance.GetFontSize(FontSize);
        }

        public void SetColor(Color color)
        {
            TryReplaceText();
            _text.color = color;
        }
    }
}