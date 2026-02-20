using UnityEngine;
using UnityEngine.XR;

/// <summary>
/// Trigger入力でRaycastし、ヒット位置にエフェクトを生成する
/// </summary>
public class TriggerRaycastEffect : MonoBehaviour
{
    [Header("有効/無効")]
    [Tooltip("この機能を有効にするか")]
    public bool isEnabled = true;

    [Header("エフェクト設定")]
    [Tooltip("ヒット時に生成するエフェクトPrefab")]
    public GameObject effectPrefab;

    [Header("Zakoヒット時クローン設定")]
    [Tooltip("Zakoヒット時に生成するPrefab（未設定なら生成しない）")]
    public GameObject cloneOnZakoHit;

    [Tooltip("Zakoヒット時クローンのDestroyまでの秒数")]
    public float cloneDestroyDelay = 2f;

    [Header("Zakoヒット時Emission設定")]
    [Tooltip("Emissionを赤へフェードするまでの秒数")]
    public float emissionFadeDuration = 0.5f;

    [Tooltip("Emissionの赤色強度")]
    public float emissionRedIntensity = 2f;

    [Header("Raycast設定")]
    [Tooltip("Raycastの最大距離")]
    public float maxDistance = 100f;

    [Tooltip("ヒット判定対象のレイヤー")]
    public LayerMask hitLayers = ~0;

    [Header("Raycast起点")]
    [Tooltip("Raycastを飛ばすTransform（未設定ならこのオブジェクト）")]
    public Transform rayOrigin;

    [Header("デバッグ")]
    public bool showDebugRay = true;

    private InputDevice _rightController;
    private InputDevice _leftController;
    private bool _wasRightTriggered;
    private bool _wasLeftTriggered;

    void Start()
    {
        if (rayOrigin == null)
        {
            rayOrigin = transform;
        }
    }

    public void Enable() => isEnabled = true;
    public void Disable() => isEnabled = false;
    public void SetEnabled(bool enabled) => isEnabled = enabled;

    void Update()
    {
        if (!isEnabled) return;
        if (effectPrefab == null) return;

        UpdateControllers();
        CheckTriggerAndShoot(ref _rightController, ref _wasRightTriggered);
        CheckTriggerAndShoot(ref _leftController, ref _wasLeftTriggered);
    }

    private void UpdateControllers()
    {
        if (!_rightController.isValid)
        {
            var devices = new System.Collections.Generic.List<InputDevice>();
            InputDevices.GetDevicesWithCharacteristics(
                InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Controller,
                devices
            );

            if (devices.Count > 0)
            {
                _rightController = devices[0];
            }
        }

        if (!_leftController.isValid)
        {
            var devices = new System.Collections.Generic.List<InputDevice>();
            InputDevices.GetDevicesWithCharacteristics(
                InputDeviceCharacteristics.Left | InputDeviceCharacteristics.Controller,
                devices
            );

            if (devices.Count > 0)
            {
                _leftController = devices[0];
            }
        }
    }

    private void CheckTriggerAndShoot(ref InputDevice controller, ref bool wasTriggered)
    {
        if (!controller.isValid)
        {
            wasTriggered = false;
            return;
        }

        if (!controller.TryGetFeatureValue(CommonUsages.triggerButton, out bool isTriggered))
        {
            wasTriggered = false;
            return;
        }

        if (isTriggered && !wasTriggered)
        {
            ShootRaycast();
        }

        wasTriggered = isTriggered;
    }

    private void ShootRaycast()
    {
        Vector3 origin = rayOrigin.position;
        Vector3 direction = rayOrigin.forward;

        if (showDebugRay)
        {
            Debug.DrawRay(origin, direction * maxDistance, Color.red, 1f);
        }

        if (Physics.Raycast(origin, direction, out RaycastHit hit, maxDistance, hitLayers))
        {
            GameObject effect = Instantiate(effectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
            Destroy(effect, 2f);
            Debug.Log($"[TriggerRaycastEffect] Hit: {hit.collider.name} at {hit.point}");

            if (hit.collider.CompareTag("zako"))
            {
                HandleZakoHit(hit.collider.gameObject, hit.point, rayOrigin.forward);
            }

            if (hit.collider.name.Contains("Core"))
            {
                HandleCoreHit(hit.collider.gameObject);
            }
        }
    }

    private void HandleCoreHit(GameObject core)
    {
        EmissionFadeOnCount emissionFade = core.GetComponent<EmissionFadeOnCount>();
        if (emissionFade != null)
        {
            emissionFade.DeactivateEmission();
            Debug.Log($"[TriggerRaycastEffect] Core hit: {core.name} - DeactivateEmission called");
        }
    }

    private void HandleZakoHit(GameObject zako, Vector3 hitPoint, Vector3 hitForward)
    {
        Animator animator = zako.GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetTrigger("Die");
        }

        if (emissionFadeDuration > 0f)
        {
            StartCoroutine(FadeEmissionToRed(zako, emissionFadeDuration, emissionRedIntensity));
        }

        if (cloneOnZakoHit != null)
        {
            Vector3 opposite = -hitForward;
            Quaternion rotation = opposite.sqrMagnitude > 0f
                ? Quaternion.LookRotation(opposite.normalized, Vector3.up)
                : Quaternion.identity;
            GameObject clone = Instantiate(cloneOnZakoHit, hitPoint, rotation);
            Destroy(clone, cloneDestroyDelay);
        }

        Destroy(zako, 2.4f);
        Debug.Log($"[TriggerRaycastEffect] Zako hit: {zako.name} - Die triggered, destroying in 1s");
    }

    private System.Collections.IEnumerator FadeEmissionToRed(GameObject target, float duration, float intensity)
    {
        var renderers = target.GetComponentsInChildren<Renderer>(true);
        if (renderers == null || renderers.Length == 0) yield break;

        var materials = new System.Collections.Generic.List<Material>();
        var original = new System.Collections.Generic.List<Color>();

        foreach (var r in renderers)
        {
            var mats = r.materials;
            foreach (var m in mats)
            {
                if (m == null) continue;
                m.EnableKeyword("_EMISSION");
                materials.Add(m);
                original.Add(m.GetColor("_EmissionColor"));
            }
        }

        if (materials.Count == 0) yield break;

        Color targetColor = Color.red * intensity;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / duration);
            for (int i = 0; i < materials.Count; i++)
            {
                materials[i].SetColor("_EmissionColor", Color.LerpUnclamped(original[i], targetColor, k));
            }
            yield return null;
        }

        for (int i = 0; i < materials.Count; i++)
        {
            materials[i].SetColor("_EmissionColor", targetColor);
        }
    }
}
