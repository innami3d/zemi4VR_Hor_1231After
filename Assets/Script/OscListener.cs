using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// クローンオブジェクトにアタッチして、Inspector上でOSCマッピングを設定できるスクリプト
/// SimpleOscReceiver.Instanceに動的に登録される
/// </summary>
public class OscListener : MonoBehaviour
{
    // データ型の定義（SimpleOscReceiverと同じ）
    public enum OscDataType
    {
        Float,
        Int,
        String,
        Trigger
    }

    [System.Serializable]
    public class OscMapping
    {
        public string address = "/example/address";
        public OscDataType type = OscDataType.Trigger;

        public UnityEvent<float> onReceivedFloat;
        public UnityEvent<int> onReceivedInt;
        public UnityEvent<string> onReceivedString;
        public UnityEvent onReceivedTrigger;
    }

    [Header("OSC Mappings")]
    public List<OscMapping> mappings = new List<OscMapping>();

    // 登録済みハンドラーの参照を保持（解除用）
    private List<(string address, SimpleOscReceiver.OscFloatHandler floatHandler)> _registeredFloatHandlers = new List<(string, SimpleOscReceiver.OscFloatHandler)>();
    private List<(string address, SimpleOscReceiver.OscIntHandler intHandler)> _registeredIntHandlers = new List<(string, SimpleOscReceiver.OscIntHandler)>();
    private List<(string address, SimpleOscReceiver.OscStringHandler stringHandler)> _registeredStringHandlers = new List<(string, SimpleOscReceiver.OscStringHandler)>();
    private List<(string address, SimpleOscReceiver.OscTriggerHandler triggerHandler)> _registeredTriggerHandlers = new List<(string, SimpleOscReceiver.OscTriggerHandler)>();

    void Start()
    {
        RegisterAllListeners();
    }

    void OnDestroy()
    {
        UnregisterAllListeners();
    }

    /// <summary>
    /// すべてのマッピングをSimpleOscReceiverに登録
    /// </summary>
    private void RegisterAllListeners()
    {
        if (SimpleOscReceiver.Instance == null)
        {
            Debug.LogWarning("[OscListener] SimpleOscReceiver.Instanceが見つかりません");
            return;
        }

        foreach (var map in mappings)
        {
            switch (map.type)
            {
                case OscDataType.Float:
                    SimpleOscReceiver.OscFloatHandler floatHandler = (value) => map.onReceivedFloat?.Invoke(value);
                    SimpleOscReceiver.Instance.RegisterFloatListener(map.address, floatHandler);
                    _registeredFloatHandlers.Add((map.address, floatHandler));
                    break;

                case OscDataType.Int:
                    SimpleOscReceiver.OscIntHandler intHandler = (value) => map.onReceivedInt?.Invoke(value);
                    SimpleOscReceiver.Instance.RegisterIntListener(map.address, intHandler);
                    _registeredIntHandlers.Add((map.address, intHandler));
                    break;

                case OscDataType.String:
                    SimpleOscReceiver.OscStringHandler stringHandler = (value) => map.onReceivedString?.Invoke(value);
                    SimpleOscReceiver.Instance.RegisterStringListener(map.address, stringHandler);
                    _registeredStringHandlers.Add((map.address, stringHandler));
                    break;

                case OscDataType.Trigger:
                    SimpleOscReceiver.OscTriggerHandler triggerHandler = () => map.onReceivedTrigger?.Invoke();
                    SimpleOscReceiver.Instance.RegisterTriggerListener(map.address, triggerHandler);
                    _registeredTriggerHandlers.Add((map.address, triggerHandler));
                    break;
            }
        }

        Debug.Log($"[OscListener] {mappings.Count}個のマッピングを登録しました");
    }

    /// <summary>
    /// すべてのリスナーを解除
    /// </summary>
    private void UnregisterAllListeners()
    {
        if (SimpleOscReceiver.Instance == null) return;

        foreach (var (address, handler) in _registeredFloatHandlers)
            SimpleOscReceiver.Instance.UnregisterFloatListener(address, handler);

        foreach (var (address, handler) in _registeredIntHandlers)
            SimpleOscReceiver.Instance.UnregisterIntListener(address, handler);

        foreach (var (address, handler) in _registeredStringHandlers)
            SimpleOscReceiver.Instance.UnregisterStringListener(address, handler);

        foreach (var (address, handler) in _registeredTriggerHandlers)
            SimpleOscReceiver.Instance.UnregisterTriggerListener(address, handler);

        _registeredFloatHandlers.Clear();
        _registeredIntHandlers.Clear();
        _registeredStringHandlers.Clear();
        _registeredTriggerHandlers.Clear();
    }
}

// --- Inspector拡張 ---
#if UNITY_EDITOR
[CustomEditor(typeof(OscListener))]
public class OscListenerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.LabelField("OSC Listener", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("SimpleOscReceiver.Instanceに動的に登録されます。Prefabやクローンに使用できます。", MessageType.Info);

        EditorGUILayout.Space();

        SerializedProperty list = serializedObject.FindProperty("mappings");

        for (int i = 0; i < list.arraySize; i++)
        {
            SerializedProperty element = list.GetArrayElementAtIndex(i);

            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Mapping {i + 1}", EditorStyles.miniBoldLabel);
            if (GUILayout.Button("Remove", GUILayout.Width(60)))
            {
                list.DeleteArrayElementAtIndex(i);
                break;
            }
            EditorGUILayout.EndHorizontal();

            SerializedProperty addressProp = element.FindPropertyRelative("address");
            EditorGUILayout.PropertyField(addressProp, new GUIContent("OSC Address"));

            SerializedProperty typeProp = element.FindPropertyRelative("type");
            EditorGUILayout.PropertyField(typeProp, new GUIContent("Data Type"));

            OscListener.OscDataType currentType = (OscListener.OscDataType)typeProp.enumValueIndex;

            SerializedProperty eventProp = null;
            switch (currentType)
            {
                case OscListener.OscDataType.Float:
                    eventProp = element.FindPropertyRelative("onReceivedFloat");
                    break;
                case OscListener.OscDataType.Int:
                    eventProp = element.FindPropertyRelative("onReceivedInt");
                    break;
                case OscListener.OscDataType.String:
                    eventProp = element.FindPropertyRelative("onReceivedString");
                    break;
                case OscListener.OscDataType.Trigger:
                    eventProp = element.FindPropertyRelative("onReceivedTrigger");
                    break;
            }

            if (eventProp != null)
            {
                EditorGUILayout.PropertyField(eventProp);
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(5);
        }

        EditorGUILayout.Space();
        if (GUILayout.Button("Add New Mapping", GUILayout.Height(30)))
        {
            list.arraySize++;
            SerializedProperty newElement = list.GetArrayElementAtIndex(list.arraySize - 1);

            newElement.FindPropertyRelative("address").stringValue = "/new/address";
            newElement.FindPropertyRelative("type").enumValueIndex = (int)OscListener.OscDataType.Trigger;

            ClearUnityEvent(newElement.FindPropertyRelative("onReceivedFloat"));
            ClearUnityEvent(newElement.FindPropertyRelative("onReceivedInt"));
            ClearUnityEvent(newElement.FindPropertyRelative("onReceivedString"));
            ClearUnityEvent(newElement.FindPropertyRelative("onReceivedTrigger"));
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void ClearUnityEvent(SerializedProperty prop)
    {
        SerializedProperty calls = prop.FindPropertyRelative("m_PersistentCalls.m_Calls");
        if (calls != null)
        {
            calls.arraySize = 0;
        }
    }
}
#endif
