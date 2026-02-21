using UnityEngine;

/// <summary>
/// Quest3のA/B/X/Yボタンを3回連打するとOSCメッセージを送信するスクリプト
/// </summary>
public class TripleTapOSCSender : MonoBehaviour
{
    [Header("OSC設定")]
    [Tooltip("送信するOSCアドレス")]
    public string oscAddress = "/button/tripleTap";
    
    [Tooltip("送信するOSC値（空の場合はアドレスのみ送信）")]
    public string oscValue = "";

    [Header("連打設定")]
    [Tooltip("連打と判定する最大間隔（秒）")]
    public float maxTapInterval = 0.5f;

    private NetworkManager networkManager;
    private int tapCount = 0;
    private float lastTapTime = 0f;

    void Start()
    {
        networkManager = GameObject.Find("NetworkManager").GetComponent<NetworkManager>();
    }

    void Update()
    {
        // Quest3のA/B/X/Yボタンを検出
        if (TryGetTappedButton(out string buttonName))
        {
            HandleButtonPress(buttonName);
        }
    }

    private bool TryGetTappedButton(out string buttonName)
    {
        if (OVRInput.GetDown(OVRInput.RawButton.A))
        {
            buttonName = "A";
            return true;
        }

        if (OVRInput.GetDown(OVRInput.RawButton.B))
        {
            buttonName = "B";
            return true;
        }

        if (OVRInput.GetDown(OVRInput.RawButton.X))
        {
            buttonName = "X";
            return true;
        }

        if (OVRInput.GetDown(OVRInput.RawButton.Y))
        {
            buttonName = "Y";
            return true;
        }

        buttonName = string.Empty;
        return false;
    }

    private void HandleButtonPress(string buttonName)
    {
        float currentTime = Time.time;

        // 前回のタップから時間が経ちすぎていればリセット
        if (currentTime - lastTapTime > maxTapInterval)
        {
            tapCount = 0;
        }

        tapCount++;
        lastTapTime = currentTime;

        Debug.Log($"{buttonName}ボタン タップ回数: {tapCount}");

        // 3回連打で送信
        if (tapCount >= 3)
        {
            SendOscMessage();
            tapCount = 0; // リセット
        }
    }

    private void SendOscMessage()
    {
        if (networkManager == null || networkManager.Client == null)
        {
            Debug.LogWarning("NetworkManagerが見つかりません");
            return;
        }

        if (string.IsNullOrEmpty(oscValue))
        {
            // 値なしでアドレスのみ送信
            networkManager.Client.Send(oscAddress);
        }
        else
        {
            // 型自動判定して送信
            if (int.TryParse(oscValue, out int i))
                networkManager.Client.Send(oscAddress, i);
            else if (float.TryParse(oscValue, out float f))
                networkManager.Client.Send(oscAddress, f);
            else
                networkManager.Client.Send(oscAddress, oscValue);
        }

        Debug.Log($"OSC送信: {oscAddress} {oscValue}");
    }
}
