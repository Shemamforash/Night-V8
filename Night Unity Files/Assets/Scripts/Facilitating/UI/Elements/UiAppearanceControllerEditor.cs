//using UI.Misc.Elements;
//using UnityEditor;
//using UnityEngine;
//
//namespace Facilitating.UI.Elements
//{
//    [CustomEditor(typeof(UiAppearanceController))]
//    public class UiAppearanceControllerEditor : Editor
//    {
//        private SerializedProperty _mainColor,
//            _secondaryColor,
//            _borderImage,
//            _universalFont;
//
//        private int _smallFontSize, _mediumFontSize, _largeFontSize, _titleFontSize;
//
//        public void OnEnable()
//        {
//            _mainColor = serializedObject.FindProperty("MainColor");
//            _secondaryColor = serializedObject.FindProperty("SecondaryColor");
//            _borderImage = serializedObject.FindProperty("BorderImage");
//            _universalFont = serializedObject.FindProperty("UniversalFont");
//
//            _smallFontSize = EditorPrefs.GetInt("Small Font Size", 0);
//            _mediumFontSize = EditorPrefs.GetInt("Medium Font Size", 0);
//            _largeFontSize = EditorPrefs.GetInt("Large Font Size", 0);
//            _titleFontSize = EditorPrefs.GetInt("Title Font Size", 0);
//        }
//
//        public override void OnInspectorGUI()
//        {
//            serializedObject.Update();
//            UiAppearanceController controller = (UiAppearanceController) target;
//            EditorGUILayout.PropertyField(_mainColor);
//            EditorGUILayout.PropertyField(_secondaryColor);
//            EditorGUILayout.PropertyField(_borderImage);
//            EditorGUILayout.PropertyField(_universalFont);
//
//            int smallFontSize = EditorGUILayout.IntField("Small Font Size:", controller.GetFontSize(EnhancedText.FontSizes.Small));
//            int mediumFontSize = EditorGUILayout.IntField("Medium Font Size:", controller.GetFontSize(EnhancedText.FontSizes.Medium));
//            int largeFontSize = EditorGUILayout.IntField("Large Font Size:", controller.GetFontSize(EnhancedText.FontSizes.Large));
//            int titleFontSize = EditorGUILayout.IntField("Title Font Size:", controller.GetFontSize(EnhancedText.FontSizes.Title));
//            controller.UpdateTextFont();
//            
//            controller.SetFontSize(EnhancedText.FontSizes.Small, smallFontSize);
//            controller.SetFontSize(EnhancedText.FontSizes.Medium, mediumFontSize);
//            controller.SetFontSize(EnhancedText.FontSizes.Large, largeFontSize);
//            controller.SetFontSize(EnhancedText.FontSizes.Title, titleFontSize);
//
//            serializedObject.ApplyModifiedProperties();
//        }
//
//        public void OnDisable()
//        {
//            EditorPrefs.SetInt("Small Font Size", _smallFontSize);
//            EditorPrefs.SetInt("Medium Font Size", _mediumFontSize);
//            EditorPrefs.SetInt("Large Font Size", _largeFontSize);
//            EditorPrefs.SetInt("Title Font Size", _titleFontSize);
//        }
//    }
//}