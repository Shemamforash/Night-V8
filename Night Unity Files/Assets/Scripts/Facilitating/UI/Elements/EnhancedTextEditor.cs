using UI.Misc.Elements;
using UnityEditor;
using UnityEngine;

namespace Facilitating.UI.Elements
{
//    [CustomEditor(typeof(EnhancedText))]
//    [CanEditMultipleObjects]
//    public class EnhancedTextEditor : Editor
//    {
//        private SerializedProperty _fontSize, _horizontalAlignment, _verticalAlignment, _text;
//
//        public void OnEnable()
//        {
//            _fontSize = serializedObject.FindProperty("FontSize");
//            _horizontalAlignment = serializedObject.FindProperty("HorizontalAlignment");
//            _verticalAlignment = serializedObject.FindProperty("VerticalAlignment");
//            _text = serializedObject.FindProperty("Text");
//        }
//        
//        public override void OnInspectorGUI()
//        {
//            serializedObject.Update();
//            EditorGUILayout.PropertyField(_fontSize);
//            EditorGUILayout.PropertyField(_horizontalAlignment);
//            EditorGUILayout.PropertyField(_verticalAlignment);
//            EditorGUILayout.PropertyField(_text, GUILayout.ExpandHeight(true));
//            EditorStyles.textField.wordWrap = true;
//            foreach (EnhancedText text in targets)
//            {
//                text.UpdateProperties();
//            }
//            serializedObject.ApplyModifiedProperties();
//        }
//    }
}