using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;

/// <summary>
/// 3つのTransformから4点のSplineを動的に生成する
/// すべてローカル座標基準
/// </summary>
[RequireComponent(typeof(SplineContainer))]
public class ThreeTransformSpline : MonoBehaviour
{
    [Header("入力Transform")]
    public Transform point1Transform;
    public Transform point2Transform;
    public Transform point3Transform;

    [Header("計算結果（読み取り専用）")]
    [SerializeField] private Vector3 splinePoint1;
    [SerializeField] private Vector3 splinePoint2;
    [SerializeField] private Vector3 splinePoint3;
    [SerializeField] private Vector3 splinePoint4;

    [Header("設定")]
    public bool autoUpdate = true;

    private SplineContainer splineContainer;

    private void Awake()
    {
        splineContainer = GetComponent<SplineContainer>();
    }

    private void Start()
    {
        GenerateSpline();
    }

    private void Update()
    {
        if (autoUpdate)
        {
            GenerateSpline();
        }
    }

    /// <summary>
    /// Splineを生成/更新する
    /// </summary>
    [ContextMenu("Generate Spline")]
    public void GenerateSpline()
    {
        if (point1Transform == null || point2Transform == null || point3Transform == null)
        {
            Debug.LogWarning("[ThreeTransformSpline] Transformが設定されていません");
            return;
        }

        if (splineContainer == null)
        {
            splineContainer = GetComponent<SplineContainer>();
            if (splineContainer == null)
            {
                splineContainer = gameObject.AddComponent<SplineContainer>();
            }
        }

        // ポイントを計算
        CalculateSplinePoints();

        // 既存のSplineをクリアして新規作成
        if (splineContainer.Splines.Count == 0)
        {
            splineContainer.AddSpline();
        }

        var spline = splineContainer.Spline;
        spline.Clear();

        // ワールド座標をSplineContainerのローカル座標に変換して追加
        Vector3 local1 = transform.InverseTransformPoint(splinePoint1);
        Vector3 local2 = transform.InverseTransformPoint(splinePoint2);
        Vector3 local3 = transform.InverseTransformPoint(splinePoint3);
        Vector3 local4 = transform.InverseTransformPoint(splinePoint4);

        spline.Add(new BezierKnot(new float3(local1.x, local1.y, local1.z)));
        spline.Add(new BezierKnot(new float3(local2.x, local2.y, local2.z)));
        spline.Add(new BezierKnot(new float3(local3.x, local3.y, local3.z)));
        spline.Add(new BezierKnot(new float3(local4.x, local4.y, local4.z)));

        Debug.Log($"[ThreeTransformSpline] Spline生成完了: 4点");
    }

    /// <summary>
    /// 4点を計算
    /// 座標はワールド座標を使用、方向はTransform1のローカル軸を基準にする
    /// </summary>
    private void CalculateSplinePoints()
    {
        // ワールド座標を取得
        Vector3 p1 = point1Transform.position;
        Vector3 p2 = point2Transform.position;
        Vector3 p3 = point3Transform.position;

        // Transform1のローカル軸を取得（方向用）
        Vector3 localForward = point1Transform.forward;  // ローカル+Z方向
        Vector3 localRight = point1Transform.right;      // ローカル+X方向

        // 1点目: Transform1の座標
        splinePoint1 = p1;

        // 2点目: p1からローカル-Z方向に進み、p2のローカルX座標と交わる点
        // p1からp2へのベクトルをTransform1のローカル空間に変換
        Vector3 p1ToP2 = p2 - p1;
        float p2LocalX = Vector3.Dot(p1ToP2, localRight);  // p2のローカルX座標
        float p2LocalZ = Vector3.Dot(p1ToP2, localForward); // p2のローカルZ座標

        // p1から-Z方向（ローカル）に進んで、p2のX座標ラインと交わる
        // -Z方向に進むので、p2LocalZ分進んだ点が2点目
        splinePoint2 = p1 - localForward * Mathf.Abs(p2LocalZ);

        // 3点目: 2点目からT2のローカルX方向と、p3からT3のローカル-Z方向の交点
        // これにより必ず直角になる
        Vector3 t2Right = point2Transform.right;     // T2のローカルX方向
        Vector3 t3Forward = point3Transform.forward; // T3のローカル+Z方向

        // レイ1: splinePoint2 + t * t2Right
        // レイ2: p3 - s * t3Forward (T3から-Z方向)
        // 2つのレイの交点を計算
        Vector3 w0 = splinePoint2 - p3;
        float a = Vector3.Dot(t2Right, t2Right);
        float b = Vector3.Dot(t2Right, -t3Forward);
        float c = Vector3.Dot(-t3Forward, -t3Forward);
        float d = Vector3.Dot(t2Right, w0);
        float e = Vector3.Dot(-t3Forward, w0);

        float denom = a * c - b * b;
        if (Mathf.Abs(denom) > 0.001f)
        {
            float t = (b * e - c * d) / denom;
            splinePoint3 = splinePoint2 + t2Right * t;
        }
        else
        {
            // 平行な場合はp2を使用
            splinePoint3 = p2;
        }

        // 4点目: Transform3の座標
        splinePoint4 = p3;
    }

    private void OnValidate()
    {
        if (Application.isPlaying && splineContainer != null)
        {
            GenerateSpline();
        }
    }

    /// <summary>
    /// デバッグ用: 各ポイントをログ出力
    /// </summary>
    [ContextMenu("Log Spline Points")]
    public void LogSplinePoints()
    {
        CalculateSplinePoints();
        Debug.Log($"[ThreeTransformSpline] 1点目: {splinePoint1}");
        Debug.Log($"[ThreeTransformSpline] 2点目: {splinePoint2}");
        Debug.Log($"[ThreeTransformSpline] 3点目: {splinePoint3}");
        Debug.Log($"[ThreeTransformSpline] 4点目: {splinePoint4}");
    }
}
