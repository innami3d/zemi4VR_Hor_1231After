using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// CueMasterからCue番号を受け取り、
/// Cue番号ごとにGameObjectのSetActiveを一括管理するスクリプト
/// </summary>
public class CueSetActiveListener : MonoBehaviour
{
    [System.Serializable]
    public class CueEntry
    {
        [Tooltip("このエントリが発火するCue番号")]
        public int cueNumber;

        [Tooltip("このCueで非アクティブにするGameObjects")]
        public List<GameObject> deactivateObjects = new List<GameObject>();

        [Tooltip("このCueでアクティブにするGameObjects")]
        public List<GameObject> activateObjects = new List<GameObject>();

    }

    [Header("Cue Entries")]
    [Tooltip("Cue番号ごとのSetActive設定")]
    [SerializeField] private List<CueEntry> cueEntries = new List<CueEntry>();

    [Header("Debug")]
    [Tooltip("デバッグログを表示")]
    [SerializeField] private bool showDebugLog = false;

    /// <summary>
    /// CueMaster.OnCueChangedから呼び出されるメソッド
    /// </summary>
    public void OnCueReceived(int cueNumber)
    {
        foreach (var entry in cueEntries)
        {
            if (entry.cueNumber != cueNumber) continue;

            // 非アクティブ化（先に実行）
            foreach (var obj in entry.deactivateObjects)
            {
                if (obj != null && obj.activeSelf)
                {
                    obj.SetActive(false);
                    if (showDebugLog) Debug.Log($"[CueSetActiveListener] Cue {cueNumber}: {obj.name} -> Inactive");
                }
            }

            // アクティブ化（後に実行）
            foreach (var obj in entry.activateObjects)
            {
                if (obj != null && !obj.activeSelf)
                {
                    obj.SetActive(true);
                    if (showDebugLog) Debug.Log($"[CueSetActiveListener] Cue {cueNumber}: {obj.name} -> Active");
                }
            }
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(CueSetActiveListener))]
public class CueSetActiveListenerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("showDebugLog"));

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Cue Entries", EditorStyles.boldLabel);

        SerializedProperty entriesProp = serializedObject.FindProperty("cueEntries");

        for (int i = 0; i < entriesProp.arraySize; i++)
        {
            SerializedProperty entry = entriesProp.GetArrayElementAtIndex(i);
            SerializedProperty cueNum = entry.FindPropertyRelative("cueNumber");
            SerializedProperty activateList = entry.FindPropertyRelative("activateObjects");
            SerializedProperty deactivateList = entry.FindPropertyRelative("deactivateObjects");

            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Cue {cueNum.intValue}", EditorStyles.boldLabel, GUILayout.Width(80));
            cueNum.intValue = EditorGUILayout.IntField(cueNum.intValue, GUILayout.Width(50));
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("X", GUILayout.Width(25)))
            {
                entriesProp.DeleteArrayElementAtIndex(i);
                break;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.PropertyField(deactivateList, new GUIContent("Deactivate"));
            EditorGUILayout.PropertyField(activateList, new GUIContent("Activate"));

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(3);
        }

        if (GUILayout.Button("+ Add Cue Entry", GUILayout.Height(25)))
        {
            int nextCue = 0;
            if (entriesProp.arraySize > 0)
            {
                var last = entriesProp.GetArrayElementAtIndex(entriesProp.arraySize - 1);
                nextCue = last.FindPropertyRelative("cueNumber").intValue + 1;
            }
            entriesProp.arraySize++;
            var newEntry = entriesProp.GetArrayElementAtIndex(entriesProp.arraySize - 1);
            newEntry.FindPropertyRelative("cueNumber").intValue = nextCue;
            newEntry.FindPropertyRelative("activateObjects").arraySize = 0;
            newEntry.FindPropertyRelative("deactivateObjects").arraySize = 0;
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
