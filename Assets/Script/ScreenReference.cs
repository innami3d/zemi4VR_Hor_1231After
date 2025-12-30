using UnityEngine;

/// <summary>
/// スクリーンPrefabにアタッチ。クローン時にPlayerTrackerへ自身を登録。
/// </summary>
public class ScreenReference : MonoBehaviour
{
    void Start()
    {
        var tracker = FindFirstObjectByType<PlayerTracker>();
        tracker?.SetReferenceFrame(transform);
    }
}
