using UnityEngine;

/// <summary>
/// SetActiveされたら指定したオブジェクトをY軸中心に回転させる
/// 180度ごとに回転数をカウントしてDebug.Logに出力する
/// </summary>
public class YAxisRotator : MonoBehaviour
{
    [Header("回転設定")]
    [Tooltip("回転させる対象のオブジェクト（未指定の場合は自身）")]
    public Transform targetObject;

    [Tooltip("回転速度（度/秒）")]
    public float rotationSpeed = 90f;

    [Header("OSC設定")]
    [Tooltip("OSC送信用")]
    public SendOSC sendOSC;

    // 累積回転角度
    private float totalRotation = 0f;

    // 現在の回転数（180度ごとにカウント）
    private int rotationCount = 0;

    // 前回のカウント時の回転数
    private int lastCountedRotation = 0;

    private void OnEnable()
    {
        // 有効化時に回転をリセット
        totalRotation = 0f;
        rotationCount = 0;
        lastCountedRotation = 0;
        Debug.Log($"[YAxisRotator] 回転開始: {gameObject.name}");
    }

    private void Update()
    {
        if (targetObject == null)
        {
            targetObject = transform;
        }

        // Y軸中心に回転
        float rotationThisFrame = rotationSpeed * Time.deltaTime;
        targetObject.Rotate(0f, rotationThisFrame, 0f, Space.Self);

        // 累積回転角度を更新
        totalRotation += Mathf.Abs(rotationThisFrame);

        // 最初180度で1回目、その後は360度ごとにカウント
        // 180, 540, 900, 1260... = 180 + 360*(n-1)
        int currentRotationCount = 0;
        if (totalRotation >= 180f)
        {
            currentRotationCount = 1 + Mathf.FloorToInt((totalRotation - 180f) / 360f);
        }

        // 新しい180度区間に達したらログ出力とOSC送信
        if (currentRotationCount > lastCountedRotation)
        {
            rotationCount = currentRotationCount;
            lastCountedRotation = currentRotationCount;
            Debug.Log($"[YAxisRotator] 回転数: {rotationCount} (累積: {totalRotation:F1}度)");

            // 8回目までOSC送信
            if (rotationCount <= 8 && sendOSC != null)
            {
                if (rotationCount == 7)
                {
                    // 7回目は特別なキュー
                    sendOSC.SendOsc("/cue/call/JumpComplete7");
                }
                else
                {
                    // 1-6回目と8回目はJumpAction
                    sendOSC.SendOsc("/event/JumpAction");
                }
            }
        }
    }

    private void OnDisable()
    {
        Debug.Log($"[YAxisRotator] 回転終了: {gameObject.name} - 最終回転数: {rotationCount}");
    }

    /// <summary>
    /// 回転数をリセット
    /// </summary>
    public void ResetRotation()
    {
        totalRotation = 0f;
        rotationCount = 0;
        lastCountedRotation = 0;
        Debug.Log($"[YAxisRotator] 回転数リセット: {gameObject.name}");
    }

    /// <summary>
    /// 現在の回転数を取得
    /// </summary>
    public int GetRotationCount()
    {
        return rotationCount;
    }
}
