using System.Linq;
using Isometric2D;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(IsometricWorld))]
public class IsometricWorldEditor : Editor
{
    private SerializedProperty _debugModeProp;

    private readonly string[] _conditionalFields = 
        { "debugSettings", "sortedObjectCount" };
    
    private readonly string[] _readonlyFields =
        { "sortedObjectCount" };

    private void OnEnable()
    {
        _debugModeProp = serializedObject.FindProperty("debugMode");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        var isDebug = _debugModeProp.boolValue;

        var iterator = serializedObject.GetIterator();
        var enterChildren = true;
        
        while (iterator.NextVisible(enterChildren))
        {
            enterChildren = false;

            if (iterator.name == "m_Script")
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.PropertyField(iterator, true);
                EditorGUI.EndDisabledGroup();
                continue;
            }

            if (IsReadOnly(iterator.name))
            {
                DrawLabelWithDisabledValue(iterator);
            }
            else
            {
                if (IsConditionalField(iterator.name))
                {
                    EditorGUI.BeginDisabledGroup(!isDebug);
                    EditorGUILayout.PropertyField(iterator, true);
                    EditorGUI.EndDisabledGroup();
                }
                else
                {
                    EditorGUILayout.PropertyField(iterator, true);
                }
            }   
        }

        serializedObject.ApplyModifiedProperties();

        if (isDebug)
            EditorGUILayout.HelpBox("Debug Mode slows the editor", MessageType.Info);
        
        EditorGUI.BeginDisabledGroup(Application.isPlaying);
        if (GUILayout.Button("Sort", GUILayout.Height(24)))
        {
            if (target is IsometricWorld isoWorld && isoWorld != null)
            {
                isoWorld.ForceSortIsoObjectsInEditor();
                isoWorld.SetDirtyDebugSort();
            }
        }
        EditorGUI.EndDisabledGroup();
    }
    
    private void DrawLabelWithDisabledValue(SerializedProperty property)
    {
        var rect = EditorGUILayout.GetControlRect();

        var labelRect = new Rect(rect.x, rect.y, EditorGUIUtility.labelWidth, rect.height);
        var valueRect = new Rect(rect.x + EditorGUIUtility.labelWidth, rect.y,
            rect.width - EditorGUIUtility.labelWidth, rect.height);

        EditorGUI.LabelField(labelRect, property.displayName);

        EditorGUI.BeginDisabledGroup(true);
        EditorGUI.PropertyField(valueRect, property, GUIContent.none, true);
        EditorGUI.EndDisabledGroup();
    }

    private bool IsConditionalField(string fieldName)
    {
        return _conditionalFields.Any(field => field == fieldName);
    }

    private bool IsReadOnly(string fieldName)
    {
        return _readonlyFields.Any(field => field == fieldName);
    }
}
