using UnityEngine;
using UnityEngine.Events;

public class TriggerChecker : MonoBehaviour
{
    [Header("フィルター設定")]
    [Tooltip("特定のタグのみに反応（空欄なら全てに反応）")]
    public string filterTag = "";

    [Tooltip("デバッグログを表示")]
    public bool showDebugLog = false;

    [Header("Trigger Events")]
    // Collider型の引数を持つUnityEventを定義
    public UnityEvent<Collider, GameObject> onTriggerEnter;
    public UnityEvent<Collider, GameObject> onTriggerStay;
    public UnityEvent<Collider, GameObject> onTriggerExit;

    [Header("Collision Events")]
    public UnityEvent<Collision, GameObject> onCollisionEnter;
    public UnityEvent<Collision, GameObject> onCollisionStay;
    public UnityEvent<Collision, GameObject> onCollisionExit;

    // コライダーが他のオブジェクトと接触した際に呼ばれるメソッド
    private void OnTriggerEnter(Collider other)
    {
        if (!PassesFilter(other.gameObject)) return;

        if (showDebugLog)
            Debug.Log($"[TriggerChecker] TriggerEnter: {other.name} (tag: {other.tag})");

        onTriggerEnter?.Invoke(other, gameObject);
    }

    private void OnTriggerStay(Collider other)
    {
        if (!PassesFilter(other.gameObject)) return;

        onTriggerStay?.Invoke(other, gameObject);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!PassesFilter(other.gameObject)) return;

        if (showDebugLog)
            Debug.Log($"[TriggerChecker] TriggerExit: {other.name}");

        onTriggerExit?.Invoke(other, gameObject);
    }

    private void OnCollisionEnter(Collision other)
    {
        if (!PassesFilter(other.gameObject)) return;

        if (showDebugLog)
            Debug.Log($"[TriggerChecker] CollisionEnter: {other.gameObject.name} (tag: {other.gameObject.tag})");

        onCollisionEnter?.Invoke(other, gameObject);
    }

    private void OnCollisionStay(Collision other)
    {
        if (!PassesFilter(other.gameObject)) return;

        onCollisionStay?.Invoke(other, gameObject);
    }

    private void OnCollisionExit(Collision other)
    {
        if (!PassesFilter(other.gameObject)) return;

        if (showDebugLog)
            Debug.Log($"[TriggerChecker] CollisionExit: {other.gameObject.name}");

        onCollisionExit?.Invoke(other, gameObject);
    }

    /// <summary>
    /// フィルター条件をパスするかチェック
    /// </summary>
    private bool PassesFilter(GameObject obj)
    {
        // タグフィルターが空なら全て通す
        if (string.IsNullOrEmpty(filterTag))
            return true;

        // タグが一致するかチェック
        return obj.CompareTag(filterTag);
    }
}
