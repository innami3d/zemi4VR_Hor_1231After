
using UnityEngine;

public class VansTracking : MonoBehaviour
{
    public GameObject target;
    void Update()
    {
        // 位置（Y=0）
        Vector3 t = target.transform.position;
        t.y = 0;
        transform.position = t;

        // Y軸回転のみ反映
        Vector3 currentRotation = transform.eulerAngles;
        currentRotation.y = target.transform.eulerAngles.y;
        transform.eulerAngles = currentRotation;
    }
}
