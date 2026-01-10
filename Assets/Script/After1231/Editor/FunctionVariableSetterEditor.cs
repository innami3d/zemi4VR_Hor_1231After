using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;

[CustomEditor(typeof(FunctionVariableSetter))]
public class FunctionVariableSetterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        var setter = (FunctionVariableSetter)target;

        // デバッグログ
        EditorGUILayout.PropertyField(serializedObject.FindProperty("showDebugLog"));
        EditorGUILayout.Space();

        // Bool Settings
        DrawSettingsArray<bool>("boolSettings", "Bool Settings", setter);
        
        // Int Settings
        DrawSettingsArray<int>("intSettings", "Int Settings", setter);
        
        // Float Settings
        DrawSettingsArray<float>("floatSettings", "Float Settings", setter);
        
        // String Settings
        DrawSettingsArray<string>("stringSettings", "String Settings", setter);

        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("onVariablesSet"));

        // テストボタン
        EditorGUILayout.Space();
        if (GUILayout.Button("Apply Settings (Test)"))
        {
            setter.ApplySettings();
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawSettingsArray<T>(string propertyName, string label, FunctionVariableSetter setter)
    {
        var arrayProperty = serializedObject.FindProperty(propertyName);
        
        EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
        EditorGUI.indentLevel++;

        int arraySize = arrayProperty.arraySize;
        int newSize = EditorGUILayout.IntField("Size", arraySize);
        if (newSize != arraySize)
        {
            arrayProperty.arraySize = newSize;
        }

        for (int i = 0; i < arrayProperty.arraySize; i++)
        {
            var element = arrayProperty.GetArrayElementAtIndex(i);
            var targetScriptProp = element.FindPropertyRelative("targetScript");
            var variableNameProp = element.FindPropertyRelative("variableName");
            var valueProp = element.FindPropertyRelative("value");

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            EditorGUILayout.PropertyField(targetScriptProp, new GUIContent("Target Script"));

            MonoBehaviour targetScript = targetScriptProp.objectReferenceValue as MonoBehaviour;
            
            if (targetScript != null)
            {
                // 対象の型に応じた変数リストを取得
                List<string> variableNames = GetVariableNames<T>(targetScript);
                
                if (variableNames.Count > 0)
                {
                    int currentIndex = variableNames.IndexOf(variableNameProp.stringValue);
                    if (currentIndex < 0) currentIndex = 0;
                    
                    int selectedIndex = EditorGUILayout.Popup("Variable Name", currentIndex, variableNames.ToArray());
                    variableNameProp.stringValue = variableNames[selectedIndex];
                }
                else
                {
                    EditorGUILayout.HelpBox($"No {typeof(T).Name} variables found", MessageType.Info);
                    variableNameProp.stringValue = "";
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Select a target script first", MessageType.Info);
            }

            EditorGUILayout.PropertyField(valueProp, new GUIContent("Value"));
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(2);
        }

        EditorGUI.indentLevel--;
        EditorGUILayout.Space();
    }

    private List<string> GetVariableNames<T>(MonoBehaviour target)
    {
        var names = new List<string>();
        var type = target.GetType();
        var targetType = typeof(T);

        // Public/Private フィールドを取得
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        foreach (var field in fields)
        {
            if (field.FieldType == targetType)
            {
                names.Add(field.Name);
            }
        }

        // プロパティを取得
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        foreach (var prop in properties)
        {
            if (prop.PropertyType == targetType && prop.CanWrite)
            {
                names.Add(prop.Name);
            }
        }

        return names;
    }
}
