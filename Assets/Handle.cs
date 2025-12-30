using UnityEngine;
using UnityEngine.Events;

public class Handle : MonoBehaviour
{
    [Header("ハンドル回転検知設定")]
    [Tooltip("通知する回転角度（度）")]
    public float notifyAngle = 90f;

    private float _previousZ;
    private float _accumulatedRotation = 0f;
    private bool _hasNotified = false;

    public GameObject HandleGrab;
    public GameObject DoorGrab;
    public GameObject Door;

    public HapticsPlayer hapticsPlayer;

    [Header("ドア回転検知設定")]
    [Tooltip("Y回転を監視するオブジェクト")]
    public GameObject doorRotationTarget;
    [Tooltip("通知するY回転角度（度、マイナス値で逆方向）")]
    public float doorNotifyAngle = -60f;

    private float _previousDoorY;
    private float _accumulatedDoorRotation = 0f;
    private bool _hasDoorNotified = false;

    void Start()
    {
        _previousZ = transform.rotation.eulerAngles.z;

        // doorRotationTargetの初期Y回転を記録
        if (doorRotationTarget != null)
        {
            _previousDoorY = doorRotationTarget.transform.rotation.eulerAngles.y;
        }
    }

    void Update()
    {
        // --- ハンドルのZ回転検知 ---
        float currentZ = transform.rotation.eulerAngles.z;

        // 差分を計算（-180〜180の範囲に正規化）
        float delta = Mathf.DeltaAngle(_previousZ, currentZ);

        // 累積回転量に加算
        _accumulatedRotation += Mathf.Abs(delta);
        // 閾値を超えたら通知
        if (!_hasNotified && _accumulatedRotation >= notifyAngle)
        {
            _hasNotified = true;
            Debug.Log($"[Handle] {notifyAngle}度回転しました！");
            Door.transform.parent = transform.parent;
            HandleGrab.SetActive(false);
            DoorGrab.SetActive(true);
            hapticsPlayer.RightHap();
            hapticsPlayer.LeftHap();
        }

        _previousZ = currentZ;

        // --- ドアのY回転検知 ---
        if (doorRotationTarget != null && !_hasDoorNotified)
        {
            float currentDoorY = doorRotationTarget.transform.rotation.eulerAngles.y;
            float doorDelta = Mathf.DeltaAngle(_previousDoorY, currentDoorY);

            // 方向付きで累積（マイナス方向を追跡）
            _accumulatedDoorRotation += doorDelta;

            // -60度以下になったら通知
            if (_accumulatedDoorRotation <= doorNotifyAngle)
            {
                _hasDoorNotified = true;
                Debug.Log($"[Handle] ドアが{doorNotifyAngle}度回転しました！");

                // 親階層のDoorsコンポーネントに通知
                Doors doors = GetComponentInParent<Doors>();
                if (doors != null)
                {
                    doors.OnDoorOpened();
                }
            }

            _previousDoorY = currentDoorY;
        }
    }
}
