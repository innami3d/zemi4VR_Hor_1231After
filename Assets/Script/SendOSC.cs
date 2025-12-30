using UnityEngine;

public class SendOSC : MonoBehaviour
{
    NetworkManager networkManager;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        networkManager = GameObject.Find("NetworkManager").GetComponent<NetworkManager>();
    }

    // Update is called once per frame
    /// <summary>
    /// "address:value" 形式で送信（UnityEvent対応）
    /// 例: "/cue/set:5" → address=/cue/set, value=5
    /// </summary>
    public void SendOsc(string addressAndValue)
    {
        var idx = addressAndValue.LastIndexOf(':');
        if (idx <= 0) { networkManager.Client.Send("/VRnotrame" + addressAndValue); return; }

        var address = "/VRnotrame" + addressAndValue.Substring(0, idx);
        var value = addressAndValue.Substring(idx + 1);

        // 型自動判定
        if (int.TryParse(value, out int i)) networkManager.Client.Send(address, i);
        else if (float.TryParse(value, out float f)) networkManager.Client.Send(address, f);
        else networkManager.Client.Send(address, value);
    }
}
