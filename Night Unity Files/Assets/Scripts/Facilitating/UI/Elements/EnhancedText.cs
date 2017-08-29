using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Facilitating.UI.Elements
{
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

        public enum HorizontalTextAlignment
        {
            Left,
            Center,
            Right
        }

        public enum VerticalTextAlignment
        {
            Top,
            Middle,
            Bottom
        }

        public FontSizes FontSize;
        public HorizontalTextAlignment HorizontalAlignment;
        public VerticalTextAlignment VerticalAlignment;
        [TextArea] public string Text = "";

        [ExecuteInEditMode]
        public void OnEnable()
        {
            _text.supportRichText = true;
            _text.alignByGeometry = true;
            _text.resizeTextForBestFit = true;
            _text.resizeTextMinSize = 10;
            _text.resizeTextMaxSize = UiAppearanceController.Instance.GetFontSize(FontSize);
            UpdateProperties();
        }

        public void SetFont(Font universalFont)
        {
            _text.font = universalFont;
            UpdateProperties();
        }

        [ExecuteInEditMode]
        public void Update()
        {
            if (!EditorApplication.isPlaying)
            {
                UpdateProperties();
            }
        }

        public void UpdateProperties()
        {
            UpdateFontSize();
            UpdateAlignment();
            _text.text = Text;
        }

        private void UpdateFontSize()
        {
            _text.fontSize = UiAppearanceController.Instance.GetFontSize(FontSize);
            _text.resizeTextMaxSize = UiAppearanceController.Instance.GetFontSize(FontSize);
        }

        private void UpdateAlignment()
        {
            switch (HorizontalAlignment)
            {
                case HorizontalTextAlignment.Left:
                    switch (VerticalAlignment)
                    {
                        case VerticalTextAlignment.Top:
                            _text.alignment = TextAnchor.UpperLeft;
                            break;
                        case VerticalTextAlignment.Middle:
                            _text.alignment = TextAnchor.MiddleLeft;
                            break;
                        case VerticalTextAlignment.Bottom:
                            _text.alignment = TextAnchor.LowerLeft;
                            break;
                    }
                    break;
                case HorizontalTextAlignment.Center:
                    switch (VerticalAlignment)
                    {
                        case VerticalTextAlignment.Top:
                            _text.alignment = TextAnchor.UpperCenter;
                            break;
                        case VerticalTextAlignment.Middle:
                            _text.alignment = TextAnchor.MiddleCenter;
                            break;
                        case VerticalTextAlignment.Bottom:
                            _text.alignment = TextAnchor.LowerCenter;
                            break;
                    }
                    break;
                case HorizontalTextAlignment.Right:
                    switch (VerticalAlignment)
                    {
                        case VerticalTextAlignment.Top:
                            _text.alignment = TextAnchor.UpperRight;
                            break;
                        case VerticalTextAlignment.Middle:
                            _text.alignment = TextAnchor.MiddleRight;
                            break;
                        case VerticalTextAlignment.Bottom:
                            _text.alignment = TextAnchor.LowerRight;
                            break;
                    }
                    break;
            }
        }

        public void SetColor(Color color)
        {
            _text.color = color;
        }
    }
}