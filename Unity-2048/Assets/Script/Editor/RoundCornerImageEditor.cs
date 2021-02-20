using UnityEngine;
using UnityEditor.UI;
using UnityEditor;
using UnityEngine.UI;

[CustomEditor(typeof(RoundCornerImage))]
public class RoundCornerImageEditor : ImageEditor
{
    private SerializedProperty propertySegment;
    private SerializedProperty propertyTopLeftRadius;
    private SerializedProperty propertyTopRightRadius;
    private SerializedProperty propertyBottomLeftRadius;
    private SerializedProperty propertyBottomRightRadius;
    private RectTransform rectTransform;

    private bool unifiedRadius = true;
    private bool maximumRadius = true;

    protected override void OnEnable()
    {
        base.OnEnable();
        propertySegment = serializedObject.FindProperty("segment");
        propertyTopLeftRadius = serializedObject.FindProperty("topLeftRadius");
        propertyTopRightRadius = serializedObject.FindProperty("topRightRadius");
        propertyBottomLeftRadius = serializedObject.FindProperty("bottomLeftRadius");
        propertyBottomRightRadius = serializedObject.FindProperty("bottomRightRadius");
        rectTransform = (target as Image).rectTransform;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour(target as MonoBehaviour), typeof(MonoScript), false);
        EditorGUI.EndDisabledGroup();
        base.OnInspectorGUI();
        EditorGUILayout.PropertyField(propertySegment);
        unifiedRadius = EditorGUILayout.Toggle("Unified Radius", unifiedRadius);
        serializedObject.ApplyModifiedProperties();
        if (unifiedRadius == false)
        {
            EditorGUILayout.PropertyField(propertyTopLeftRadius);
            EditorGUILayout.PropertyField(propertyTopRightRadius);
            EditorGUILayout.PropertyField(propertyBottomLeftRadius);
            EditorGUILayout.PropertyField(propertyBottomRightRadius);
        }
        else
        {
            maximumRadius = EditorGUILayout.Toggle("Maximum Radius", maximumRadius);
            serializedObject.ApplyModifiedProperties();

            if (maximumRadius == true)
            {
                float size = Mathf.Min(rectTransform.rect.width, rectTransform.rect.height);
                propertyTopLeftRadius.doubleValue = size * 0.5f;
            }
            EditorGUILayout.PropertyField(propertyTopLeftRadius, new GUIContent("Radius"));
            propertyTopRightRadius.doubleValue = propertyTopLeftRadius.doubleValue;
            propertyBottomLeftRadius.doubleValue = propertyTopLeftRadius.doubleValue;
            propertyBottomRightRadius.doubleValue = propertyTopLeftRadius.doubleValue;
        }
        propertyTopLeftRadius.doubleValue = propertyTopLeftRadius.doubleValue < 0 ? 0 : propertyTopLeftRadius.doubleValue;
        propertyTopRightRadius.doubleValue = propertyTopRightRadius.doubleValue < 0 ? 0 : propertyTopRightRadius.doubleValue;
        propertyBottomLeftRadius.doubleValue = propertyBottomLeftRadius.doubleValue < 0 ? 0 : propertyBottomLeftRadius.doubleValue;
        propertyBottomRightRadius.doubleValue = propertyBottomRightRadius.doubleValue < 0 ? 0 : propertyBottomRightRadius.doubleValue;
        serializedObject.ApplyModifiedProperties();
    }

    [MenuItem("GameObject/UI/Round Corner Image", false, -100)]
    private static void Create()
    {
        UGUIEditorUtil.CreateUGUIObject<RoundCornerImage>("RoundCornerImage");
    }
}