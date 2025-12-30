using UnityEngine;
using System.Collections;

public class Doors : MonoBehaviour
{
    public GameObject Door;
    public GameObject Wall;

    [Header("生成設定")]
    public int count = 5;

    [Header("移動設定")]
    public float moveDistance = 1.5f;
    public float moveDuration = 1f;

    private Vector3 _originalLocalPosition;
    private bool _isMoving = false;

    void Start()
    {
        _originalLocalPosition = transform.localPosition;
        Generate();
    }

    /// <summary>
    /// ドアが開かれた時に呼ばれる
    /// </summary>
    public void OnDoorOpened()
    {
        if (_isMoving) return;
        StartCoroutine(MoveAndReset());
    }

    private IEnumerator MoveAndReset()
    {
        _isMoving = true;

        Vector3 startPos = transform.localPosition;
        Vector3 endPos = startPos + new Vector3(moveDistance, 0f, 0f);
        float elapsed = 0f;

        // 1秒かけてX方向に移動
        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / moveDuration);
            transform.localPosition = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        transform.localPosition = endPos;
        Debug.Log("[Doors] 移動完了、リセット開始");

        // 元の座標に戻す
        transform.localPosition = _originalLocalPosition;

        // 子要素（Door/Wall）をすべて削除
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }

        // 新しく生成
        Generate();

        _isMoving = false;
    }

    void Generate()
    {
        int layers = 2;
        float layerHeight = 0.85f;
        int totalSlots = count * layers;

        // ドアを配置するランダムなスロットを決定（全スロットから1つ）
        int doorSlot = Random.Range(0, totalSlots);

        for (int layer = 0; layer < layers; layer++)
        {
            float baseY = 0.533f + (layer * layerHeight);

            for (int i = 0; i < count; i++)
            {
                int currentSlot = layer * count + i;

                // Z座標: index 0 = 0.356, index 1 = 1.0, ... (0.644刻み)
                float z = -0.356f + (i * -0.644f);
                Vector3 localPos = new Vector3(0f, baseY, z);

                // ランダムに選ばれたスロットにはDoor、それ以外はWall
                GameObject prefab = (currentSlot == doorSlot) ? Door : Wall;
                GameObject instance = Instantiate(prefab, transform);
                instance.transform.localPosition = localPos;
                instance.transform.localRotation = Quaternion.identity;
                instance.name = (currentSlot == doorSlot) ? $"Door_L{layer}_{i}" : $"Wall_L{layer}_{i}";
            }
        }

        int doorLayer = doorSlot / count;
        int doorIndex = doorSlot % count;
        Debug.Log($"[Doors] 生成完了: Door位置 = Layer {doorLayer}, Index {doorIndex}");
    }
}
