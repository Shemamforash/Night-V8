using UnityEditor;

namespace UI.Misc.Elements
{
    [CustomEditor(typeof(EnhancedButton))]
    [CanEditMultipleObjects]
    public class EnhancedButtonEditor : Editor
    {
        private SerializedProperty useBorder;

        public void OnEnable()
        {
            useBorder = serializedObject.FindProperty("UseBorder");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(useBorder);
            serializedObject.ApplyModifiedProperties();
        }
    }
}