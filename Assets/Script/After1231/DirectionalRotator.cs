using UnityEngine;
using System.Collections;

/// <summary>
/// 実行時に一度だけ、指定したオブジェクトの方向を正面として
/// 初期Y軸回転を基準に90度単位で回転するスクリプト
/// 例：初期回転10度なら、10, 100, 190, 280度が選択肢
/// </summary>
public class DirectionalRotator : MonoBehaviour
{
    [Tooltip("正面方向を示すオブジェクト（直接指定）")]
    public Transform forwardTarget;

    [Tooltip("検索するオブジェクトの名前（forwardTargetが未設定の場合に使用）")]
    public string searchObjectName;

    // 初期のY軸回転を保存
    private float baseAngle;

    private void Start()
    {
        // 初期のY軸回転を保存
        baseAngle = transform.eulerAngles.y;

        if (forwardTarget != null)
        {
            // 直接指定されている場合は即座に回転
            ApplyRotation();
        }
        else if (!string.IsNullOrEmpty(searchObjectName))
        {
            // 名前で検索する場合はコルーチンを開始
            StartCoroutine(SearchAndApply());
        }
    }

    /// <summary>
    /// 0.5秒ごとに最大4回、オブジェクトを検索する
    /// </summary>
    private IEnumerator SearchAndApply()
    {
        for (int i = 0; i < 4; i++)
        {
            GameObject found = GameObject.Find(searchObjectName);

            if (found != null)
            {
                forwardTarget = found.transform;
                ApplyRotation();
                yield break; // 見つかったら終了
            }

            yield return new WaitForSeconds(0.5f);
        }

        Debug.LogWarning($"DirectionalRotator: '{searchObjectName}' が見つかりませんでした");
    }

    /// <summary>
    /// ターゲット方向に90度単位で回転を適用
    /// </summary>
    private void ApplyRotation()
    {
        if (forwardTarget == null) return;

        // ターゲットへの方向を計算
        Vector3 direction = forwardTarget.position - transform.position;
        direction.y = 0; // Y軸は無視して水平方向のみ

        if (direction.sqrMagnitude < 0.001f) return;

        // ターゲット方向の角度
        float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;

        // 初期角度を基準に90度単位でスナップ
        // baseAngle, baseAngle+90, baseAngle+180, baseAngle+270 の中から最も近いものを選択
        float bestAngle = baseAngle;
        float minDiff = float.MaxValue;

        for (int i = 0; i < 4; i++)
        {
            float candidateAngle = baseAngle + i * 90f;
            float diff = Mathf.Abs(Mathf.DeltaAngle(targetAngle, candidateAngle));

            if (diff < minDiff)
            {
                minDiff = diff;
                bestAngle = candidateAngle;
            }
        }

        // Y軸回転を適用
        transform.rotation = Quaternion.Euler(transform.eulerAngles.x, bestAngle, transform.eulerAngles.z);
    }

    // デバッグ用：方向を可視化
    private void OnDrawGizmosSelected()
    {
        if (forwardTarget == null) return;

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, forwardTarget.position);
        Gizmos.DrawSphere(forwardTarget.position, 0.1f);
    }
}
