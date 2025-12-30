using UnityEngine;

/// <summary>
/// ネットワーク設定を保持するScriptableObject
/// </summary>
[CreateAssetMenu(fileName = "NetworkConfig", menuName = "VR/Network Config")]
public class NetworkConfig : ScriptableObject
{
    [Header("OSC設定")]
    [SerializeField] private string targetIP = "127.0.0.1";
    [SerializeField] private int sendPort = 17200;
    [SerializeField] private int receivePort = 20001;
    
    [Header("送信設定")]
    [SerializeField] private float sendInterval = 0.033f; // ~30fps
    
    // プロパティ
    public string TargetIP => targetIP;
    public int SendPort => sendPort;
    public int ReceivePort => receivePort;
    public float SendInterval => sendInterval;
}
