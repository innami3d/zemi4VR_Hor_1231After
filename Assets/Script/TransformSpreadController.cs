using UnityEngine;

/// <summary>
/// 子要素のTransformを親を中心にXZ方向で広げたり縮めたりするコントローラー
/// Editor時でも動作し、Play中の変更は停止後も反映される
/// </summary>
[ExecuteAlways]
public class TransformSpreadController : MonoBehaviour
{
    [Header("広がり設定")]
    [Tooltip("X方向の広がり係数 (1.0 = デフォルト位置, 2.0 = 2倍の距離)")]
    [Range(0f, 5.0f)]
    public float spreadFactorX = 1.0f;

    [Tooltip("Z方向の広がり係数 (1.0 = デフォルト位置, 2.0 = 2倍の距離)")]
    [Range(0f, 5.0f)]
    public float spreadFactorZ = 1.0f;

    [Header("子要素のTransform (自動取得)")]
    [SerializeField] private Transform simoFront;
    [SerializeField] private Transform centerFront;
    [SerializeField] private Transform kamiFront;
    [SerializeField] private Transform simoBack;
    [SerializeField] private Transform centerBack;
    [SerializeField] private Transform kamiBack;

    // 各子要素の初期オフセット位置（親からの相対位置）
    private Vector3 simoFrontBaseOffset;
    private Vector3 centerFrontBaseOffset;
    private Vector3 kamiFrontBaseOffset;
    private Vector3 simoBackBaseOffset;
    private Vector3 centerBackBaseOffset;
    private Vector3 kamiBackBaseOffset;

    // 初期化済みフラグ
    private bool isInitialized = false;

    // 前回のspreadFactor値を保存（変更検出用）
    private float lastSpreadFactorX;
    private float lastSpreadFactorZ;

    private void OnEnable()
    {
        Initialize();
    }

    private void OnValidate()
    {
        // Inspectorで値が変更されたときに更新
        if (isInitialized)
        {
            UpdateTransformPositions();
        }
    }

    private void Update()
    {
        // spreadFactorが変更された場合のみ更新
        if (isInitialized && (!Mathf.Approximately(lastSpreadFactorX, spreadFactorX) ||
                              !Mathf.Approximately(lastSpreadFactorZ, spreadFactorZ)))
        {
            UpdateTransformPositions();
            lastSpreadFactorX = spreadFactorX;
            lastSpreadFactorZ = spreadFactorZ;
        }
    }

    public void SetSpreadFactorX(float value)
    {
        spreadFactorX = value;
        UpdateTransformPositions();
    }

    public void SetSpreadFactorZ(float value)
    {
        spreadFactorZ = value;
        UpdateTransformPositions();
    }

    /// <summary>
    /// 子要素を検索し、初期オフセットを記録する
    /// </summary>
    private void Initialize()
    {
        // 子要素を名前で検索
        simoFront = transform.Find("SimoFront");
        centerFront = transform.Find("CenterFront");
        kamiFront = transform.Find("KamiFront");
        simoBack = transform.Find("SimoBack");
        centerBack = transform.Find("CenterBack");
        kamiBack = transform.Find("KamiBack");

        // すべての子要素が見つかったか確認
        if (simoFront == null || centerFront == null || kamiFront == null ||
            simoBack == null || centerBack == null || kamiBack == null)
        {
            Debug.LogWarning($"[TransformSpreadController] 一部の子要素が見つかりません。必要な子要素: SimoFront, CenterFront, KamiFront, SimoBack, CenterBack, KamiBack", this);
            isInitialized = false;
            return;
        }

        // 初期オフセットを記録（spreadFactor=1.0のときの位置として保存）
        // 現在のspreadFactorを考慮して基準オフセットを計算
        float currentFactorX = spreadFactorX > 0.01f ? spreadFactorX : 1.0f;
        float currentFactorZ = spreadFactorZ > 0.01f ? spreadFactorZ : 1.0f;

        simoFrontBaseOffset = GetScaledOffset(simoFront.localPosition, currentFactorX, currentFactorZ);
        centerFrontBaseOffset = GetScaledOffset(centerFront.localPosition, currentFactorX, currentFactorZ);
        kamiFrontBaseOffset = GetScaledOffset(kamiFront.localPosition, currentFactorX, currentFactorZ);
        simoBackBaseOffset = GetScaledOffset(simoBack.localPosition, currentFactorX, currentFactorZ);
        centerBackBaseOffset = GetScaledOffset(centerBack.localPosition, currentFactorX, currentFactorZ);
        kamiBackBaseOffset = GetScaledOffset(kamiBack.localPosition, currentFactorX, currentFactorZ);

        lastSpreadFactorX = spreadFactorX;
        lastSpreadFactorZ = spreadFactorZ;
        isInitialized = true;
    }

    /// <summary>
    /// 位置からスケーリングされた基準オフセットを計算
    /// </summary>
    private Vector3 GetScaledOffset(Vector3 position, float factorX, float factorZ)
    {
        return new Vector3(position.x / factorX, position.y, position.z / factorZ);
    }

    // X軸の係数: Simo=80%, Center=0%, Kami=-80%
    private const float SIMO_X_RATIO = 0.8f;
    private const float CENTER_X_RATIO = 0.0f;
    private const float KAMI_X_RATIO = -0.8f;

    // Z軸の係数: Front=100%, Back=40%
    private const float FRONT_Z_RATIO = 1.0f;
    private const float BACK_Z_RATIO = 0.4f;

    /// <summary>
    /// spreadFactorに基づいて全てのTransformの位置を更新
    /// </summary>
    private void UpdateTransformPositions()
    {
        if (!isInitialized) return;

        // SimoFront: X=80%, Z=100%
        ApplySpread(simoFront, simoFrontBaseOffset, SIMO_X_RATIO, FRONT_Z_RATIO);
        // CenterFront: X=0%, Z=100%
        ApplySpread(centerFront, centerFrontBaseOffset, CENTER_X_RATIO, FRONT_Z_RATIO);
        // KamiFront: X=-80%, Z=100%
        ApplySpread(kamiFront, kamiFrontBaseOffset, KAMI_X_RATIO, FRONT_Z_RATIO);
        // SimoBack: X=80%, Z=40%
        ApplySpread(simoBack, simoBackBaseOffset, SIMO_X_RATIO, BACK_Z_RATIO);
        // CenterBack: X=0%, Z=40%
        ApplySpread(centerBack, centerBackBaseOffset, CENTER_X_RATIO, BACK_Z_RATIO);
        // KamiBack: X=-80%, Z=40%
        ApplySpread(kamiBack, kamiBackBaseOffset, KAMI_X_RATIO, BACK_Z_RATIO);

#if UNITY_EDITOR
        // Editorモードでの変更を確実に保存するためにDirtyフラグを設定
        if (!Application.isPlaying)
        {
            UnityEditor.EditorUtility.SetDirty(this);
            foreach (Transform child in new Transform[] { simoFront, centerFront, kamiFront, simoBack, centerBack, kamiBack })
            {
                if (child != null)
                {
                    UnityEditor.EditorUtility.SetDirty(child);
                }
            }
        }
#endif
    }

    /// <summary>
    /// 個別のTransformにspreadFactorを適用
    /// </summary>
    /// <param name="target">対象Transform</param>
    /// <param name="baseOffset">基準オフセット（Y座標の保持に使用）</param>
    /// <param name="xRatio">X軸の適用割合</param>
    /// <param name="zRatio">Z軸の適用割合</param>
    private void ApplySpread(Transform target, Vector3 baseOffset, float xRatio, float zRatio)
    {
        if (target == null) return;

        // spreadFactor=0で中心に集まり、値が大きくなるほど広がる
        // Y座標は元の位置を維持
        Vector3 newPosition = new Vector3(
            spreadFactorX * xRatio,
            baseOffset.y, // Yは変更しない
            spreadFactorZ * zRatio
        );

        target.localPosition = newPosition;
    }

    /// <summary>
    /// 現在の位置を基準位置として再設定する
    /// </summary>
    [ContextMenu("現在の位置を基準として設定")]
    public void SetCurrentAsBase()
    {
        Initialize();
        Debug.Log("[TransformSpreadController] 現在の位置を基準位置として再設定しました", this);
    }

    /// <summary>
    /// 強制的に位置を更新する
    /// </summary>
    [ContextMenu("位置を強制更新")]
    public void ForceUpdatePositions()
    {
        if (!isInitialized)
        {
            Initialize();
        }
        UpdateTransformPositions();
    }
}
