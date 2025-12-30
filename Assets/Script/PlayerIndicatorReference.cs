using UnityEngine;

/// <summary>
/// PlayerIndicatorにアタッチ。生成時にPlayerTrackerへ自身を登録。
/// ScreenReferenceと同じパターン。
/// </summary>
public class PlayerIndicatorReference : MonoBehaviour
{
    void Start()
    {
        var tracker = FindFirstObjectByType<PlayerTracker>();
        tracker?.SetTargetObject(gameObject);
    }
}
