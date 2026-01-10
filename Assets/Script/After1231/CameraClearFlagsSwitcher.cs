using UnityEngine;

/// <summary>
/// GameObjectがアクティブになったらCenterEyeAnchorのCameraのClearFlagsをSolidColorに変更し、
/// 非アクティブになったらSkyboxに戻すコンポーネント
/// </summary>
public class CameraClearFlagsSwitcher : MonoBehaviour
{
    private Camera targetCamera;

    private void OnEnable()
    {
        // CenterEyeAnchorを検索してCameraを取得
        GameObject centerEye = GameObject.Find("CenterEyeAnchor");
        if (centerEye != null)
        {
            targetCamera = centerEye.GetComponent<Camera>();
            if (targetCamera != null)
            {
                targetCamera.clearFlags = CameraClearFlags.SolidColor;
                Debug.Log("[CameraClearFlagsSwitcher] Camera ClearFlags set to SolidColor");
            }
            else
            {
                Debug.LogWarning("[CameraClearFlagsSwitcher] CenterEyeAnchor found but no Camera component");
            }
        }
        else
        {
            Debug.LogWarning("[CameraClearFlagsSwitcher] CenterEyeAnchor not found");
        }
    }

    private void OnDisable()
    {
        // キャッシュしたカメラがあればSkyboxに戻す
        if (targetCamera != null)
        {
            targetCamera.clearFlags = CameraClearFlags.Skybox;
            Debug.Log("[CameraClearFlagsSwitcher] Camera ClearFlags set to Skybox");
        }
    }
}
