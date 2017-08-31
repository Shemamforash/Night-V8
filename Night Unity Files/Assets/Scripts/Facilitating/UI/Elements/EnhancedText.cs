using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Facilitating.UI.Elements
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Text))]
    public class EnhancedText : MonoBehaviour
    {
        private Text _text;

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
            _text = gameObject.GetComponent<Text>();
            _text.supportRichText = true;
            _text.alignByGeometry = true;
            _text.resizeTextForBestFit = true;
            _text.resizeTextMinSize = 10;
            _text.resizeTextMaxSize = UiAppearanceController.Instance.GetFontSize(FontSize);
            UpdateFontSize();
            _text.font = UiAppearanceController.Instance.UniversalFont;
        }

        public void SetFont(Font universalFont)
        {
            _text = gameObject.GetComponent<Text>();
            _text.font = universalFont;
            UpdateFontSize();
        }

        public void Update()
        {
            if (!EditorApplication.isPlayingOrWillChangePlaymode)
            {
                UpdateFontSize();
            }
        }

        private void UpdateFontSize()
        {
            _text.fontSize = UiAppearanceController.Instance.GetFontSize(FontSize);
            _text.resizeTextMaxSize = UiAppearanceController.Instance.GetFontSize(FontSize);
        }

        public void SetColor(Color color)
        {
            _text.color = color;
        }
    }
}