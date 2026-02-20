using UnityEngine;

/// <summary>
/// Start時に名前で対象オブジェクトを取得し、常にその方向を向く。
/// </summary>
public class LookAtTargetByName : MonoBehaviour
{
    [Header("対象設定")]
    [Tooltip("Start時に検索する対象オブジェクト名")]
    public string targetObjectName;

    private Transform _target;

    void Start()
    {
        if (string.IsNullOrWhiteSpace(targetObjectName))
        {
            Debug.LogWarning("[LookAtTargetByName] targetObjectName is empty.");
            return;
        }

        GameObject targetObj = GameObject.Find(targetObjectName);
        if (targetObj == null)
        {
            Debug.LogWarning($"[LookAtTargetByName] Target not found: {targetObjectName}");
            return;
        }

        _target = targetObj.transform;
    }

    void Update()
    {
        if (_target == null) return;

        Vector3 targetPosition = _target.position;
        targetPosition.y = transform.position.y;
        transform.LookAt(targetPosition);
    }
}
