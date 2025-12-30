using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

// Unity 6000.0.x 動作確認済み
// Add New Mappingボタンを一番下に配置
public class SimpleOscReceiver : MonoBehaviour
{
    // シングルトンインスタンス
    public static SimpleOscReceiver Instance { get; private set; }

    [Header("OSC Settings")]
    [Tooltip("受信ポート番号")]
    public int port = 9000;

    [Tooltip("デバッグログを表示")]
    public bool showDebugLog = false;

    // データ型の定義
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
        public string address = "/avatar/parameters/Example";
        public OscDataType type = OscDataType.Float;

        public UnityEvent<float> onReceivedFloat;
        public UnityEvent<int> onReceivedInt;
        public UnityEvent<string> onReceivedString;
        public UnityEvent onReceivedTrigger;
    }

    // 動的リスナー用のデリゲート
    public delegate void OscFloatHandler(float value);
    public delegate void OscIntHandler(int value);
    public delegate void OscStringHandler(string value);
    public delegate void OscTriggerHandler();

    // 動的に登録されるリスナー
    private Dictionary<string, List<OscFloatHandler>> _floatListeners = new Dictionary<string, List<OscFloatHandler>>();
    private Dictionary<string, List<OscIntHandler>> _intListeners = new Dictionary<string, List<OscIntHandler>>();
    private Dictionary<string, List<OscStringHandler>> _stringListeners = new Dictionary<string, List<OscStringHandler>>();
    private Dictionary<string, List<OscTriggerHandler>> _triggerListeners = new Dictionary<string, List<OscTriggerHandler>>();

    public List<OscMapping> mappings = new List<OscMapping>();

    private ConcurrentQueue<OscMessage> _messageQueue = new ConcurrentQueue<OscMessage>();
    private UdpClient _udpClient;
    private Thread _receiveThread;
    private bool _isRunning = false;

    private struct OscMessage
    {
        public string Address;
        public object Value;
    }

    void Awake()
    {
        // シングルトン設定
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[SimpleOSC] 既にインスタンスが存在します。このインスタンスは破棄されます。");
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        StartServer(port);
    }

    #region 動的リスナー登録API

    /// <summary>
    /// Float型のリスナーを登録
    /// </summary>
    public void RegisterFloatListener(string address, OscFloatHandler handler)
    {
        if (!_floatListeners.ContainsKey(address))
            _floatListeners[address] = new List<OscFloatHandler>();
        _floatListeners[address].Add(handler);
        if (showDebugLog) Debug.Log($"[SimpleOSC] Float listener registered: {address}");
    }

    /// <summary>
    /// Float型のリスナーを解除
    /// </summary>
    public void UnregisterFloatListener(string address, OscFloatHandler handler)
    {
        if (_floatListeners.ContainsKey(address))
            _floatListeners[address].Remove(handler);
    }

    /// <summary>
    /// Int型のリスナーを登録
    /// </summary>
    public void RegisterIntListener(string address, OscIntHandler handler)
    {
        if (!_intListeners.ContainsKey(address))
            _intListeners[address] = new List<OscIntHandler>();
        _intListeners[address].Add(handler);
        if (showDebugLog) Debug.Log($"[SimpleOSC] Int listener registered: {address}");
    }

    /// <summary>
    /// Int型のリスナーを解除
    /// </summary>
    public void UnregisterIntListener(string address, OscIntHandler handler)
    {
        if (_intListeners.ContainsKey(address))
            _intListeners[address].Remove(handler);
    }

    /// <summary>
    /// String型のリスナーを登録
    /// </summary>
    public void RegisterStringListener(string address, OscStringHandler handler)
    {
        if (!_stringListeners.ContainsKey(address))
            _stringListeners[address] = new List<OscStringHandler>();
        _stringListeners[address].Add(handler);
        if (showDebugLog) Debug.Log($"[SimpleOSC] String listener registered: {address}");
    }

    /// <summary>
    /// String型のリスナーを解除
    /// </summary>
    public void UnregisterStringListener(string address, OscStringHandler handler)
    {
        if (_stringListeners.ContainsKey(address))
            _stringListeners[address].Remove(handler);
    }

    /// <summary>
    /// Trigger型のリスナーを登録
    /// </summary>
    public void RegisterTriggerListener(string address, OscTriggerHandler handler)
    {
        if (!_triggerListeners.ContainsKey(address))
            _triggerListeners[address] = new List<OscTriggerHandler>();
        _triggerListeners[address].Add(handler);
        if (showDebugLog) Debug.Log($"[SimpleOSC] Trigger listener registered: {address}");
    }

    /// <summary>
    /// Trigger型のリスナーを解除
    /// </summary>
    public void UnregisterTriggerListener(string address, OscTriggerHandler handler)
    {
        if (_triggerListeners.ContainsKey(address))
            _triggerListeners[address].Remove(handler);
    }

    #endregion

    void Update()
    {
        while (_messageQueue.TryDequeue(out OscMessage message))
        {
            // Inspector設定のマッピングを処理
            foreach (var map in mappings)
            {
                if (map.address == message.Address)
                {
                    DispatchEvent(map, message.Value);
                }
            }

            // 動的に登録されたリスナーを処理
            DispatchDynamicListeners(message.Address, message.Value);
        }
    }

    /// <summary>
    /// 動的に登録されたリスナーにメッセージをディスパッチ
    /// </summary>
    private void DispatchDynamicListeners(string address, object value)
    {
        try
        {
            // Floatリスナー
            if (_floatListeners.ContainsKey(address))
            {
                float floatValue = Convert.ToSingle(value);
                foreach (var handler in _floatListeners[address])
                {
                    handler?.Invoke(floatValue);
                }
            }

            // Intリスナー
            if (_intListeners.ContainsKey(address))
            {
                int intValue = Convert.ToInt32(value);
                foreach (var handler in _intListeners[address])
                {
                    handler?.Invoke(intValue);
                }
            }

            // Stringリスナー
            if (_stringListeners.ContainsKey(address))
            {
                string stringValue = value.ToString();
                foreach (var handler in _stringListeners[address])
                {
                    handler?.Invoke(stringValue);
                }
            }

            // Triggerリスナー
            if (_triggerListeners.ContainsKey(address))
            {
                foreach (var handler in _triggerListeners[address])
                {
                    handler?.Invoke();
                }
            }
        }
        catch (Exception e)
        {
            if (showDebugLog) Debug.LogWarning($"[SimpleOSC] Dynamic listener error: {e.Message}");
        }
    }

    private void DispatchEvent(OscMapping map, object value)
    {
        try
        {
            switch (map.type)
            {
                case OscDataType.Float:
                    map.onReceivedFloat.Invoke(Convert.ToSingle(value));
                    break;
                case OscDataType.Int:
                    map.onReceivedInt.Invoke(Convert.ToInt32(value));
                    break;
                case OscDataType.String:
                    map.onReceivedString.Invoke(value.ToString());
                    break;
                case OscDataType.Trigger:
                    map.onReceivedTrigger.Invoke();
                    break;
            }
        }
        catch { }
    }

    public void StartServer(int _port)
    {
        if (_isRunning) return;
        try
        {
            port = _port;
            _udpClient = new UdpClient(port);
            _isRunning = true;
            _receiveThread = new Thread(ReceiveLoop);
            _receiveThread.IsBackground = true;
            _receiveThread.Start();
            Debug.Log($"[SimpleOSC] Started on port {port}");
        }
        catch (Exception e) { Debug.LogError($"[SimpleOSC] Failed: {e.Message}"); }
    }

    private void ReceiveLoop()
    {
        IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, port);
        while (_isRunning)
        {
            try
            {
                byte[] data = _udpClient.Receive(ref remoteEndPoint);
                if (data.Length > 0) ParseOsc(data);
            }
            catch (SocketException) { break; }
            catch { }
        }
    }

    private void ParseOsc(byte[] data)
    {
        try
        {
            int index = 0;
            string address = ReadString(data, ref index);
            if (string.IsNullOrEmpty(address)) return;

            string typeTag = "";
            if (index < data.Length && data[index] == ',') typeTag = ReadString(data, ref index);

            object value = 0f;

            if (typeTag.Length > 1)
            {
                switch (typeTag[1])
                {
                    case 'f': value = ReadFloat(data, ref index); break;
                    case 'i': value = ReadInt(data, ref index); break;
                    case 's': value = ReadString(data, ref index); break;
                    case 'T': value = 1.0f; break;
                    case 'F': value = 0.0f; break;
                }
            }

            _messageQueue.Enqueue(new OscMessage { Address = address, Value = value });
            if (showDebugLog) Debug.Log($"[OSC] {address} : {value}");
        }
        catch { }
    }

    private string ReadString(byte[] data, ref int index)
    {
        int start = index;
        while (index < data.Length && data[index] != 0) index++;
        if (index >= data.Length) return "";
        string s = Encoding.UTF8.GetString(data, start, index - start);
        index++;
        index += (4 - (index % 4)) % 4;
        return s;
    }
    private float ReadFloat(byte[] data, ref int index)
    {
        if (index + 4 > data.Length) return 0;
        byte[] b = { data[index + 3], data[index + 2], data[index + 1], data[index] };
        index += 4;
        return BitConverter.ToSingle(b, 0);
    }
    private int ReadInt(byte[] data, ref int index)
    {
        if (index + 4 > data.Length) return 0;
        byte[] b = { data[index + 3], data[index + 2], data[index + 1], data[index] };
        index += 4;
        return BitConverter.ToInt32(b, 0);
    }

    void OnDestroy() => Cleanup();
    void OnApplicationQuit() => Cleanup();
    private void Cleanup()
    {
        _isRunning = false;
        if (_udpClient != null) { _udpClient.Close(); _udpClient = null; }
        if (_receiveThread != null) { if (!_receiveThread.Join(100)) _receiveThread.Abort(); }
    }
}

// --- Inspector拡張 ---
#if UNITY_EDITOR
[CustomEditor(typeof(SimpleOscReceiver))]
public class SimpleOscReceiverEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("port"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("showDebugLog"));

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Address Mappings", EditorStyles.boldLabel);

        SerializedProperty list = serializedObject.FindProperty("mappings");

        // --- 1. まず既存のリストを表示 ---
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

            SimpleOscReceiver.OscDataType currentType =
                (SimpleOscReceiver.OscDataType)typeProp.enumValueIndex;

            SerializedProperty eventProp = null;
            switch (currentType)
            {
                case SimpleOscReceiver.OscDataType.Float:
                    eventProp = element.FindPropertyRelative("onReceivedFloat");
                    break;
                case SimpleOscReceiver.OscDataType.Int:
                    eventProp = element.FindPropertyRelative("onReceivedInt");
                    break;
                case SimpleOscReceiver.OscDataType.String:
                    eventProp = element.FindPropertyRelative("onReceivedString");
                    break;
                case SimpleOscReceiver.OscDataType.Trigger:
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

        // --- 2. リスト表示のあとにボタンを配置 ---
        EditorGUILayout.Space();
        if (GUILayout.Button("Add New Mapping", GUILayout.Height(30)))
        {
            list.arraySize++;
            SerializedProperty newElement = list.GetArrayElementAtIndex(list.arraySize - 1);

            newElement.FindPropertyRelative("address").stringValue = "/new/address";
            newElement.FindPropertyRelative("type").enumValueIndex = (int)SimpleOscReceiver.OscDataType.Float;

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