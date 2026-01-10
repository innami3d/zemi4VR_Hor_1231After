using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// GameObjectがActiveになったタイミングで指定したスクリプトの変数を変更する
/// </summary>
public class OnEnableVariableSetter : MonoBehaviour
{
    [Tooltip("このスクリプトの機能を有効にする")]
    public bool isEnabled = true;

    [Tooltip("デバッグログを表示")]
    public bool showDebugLog = false;
    [System.Serializable]
    public class BoolSetting
    {
        [Tooltip("対象のMonoBehaviour")]
        public MonoBehaviour targetScript;
        [Tooltip("変更する変数名")]
        public string variableName;
        [Tooltip("設定する値")]
        public bool value;
    }
    public BoolSetting[] boolSettings;
    [System.Serializable]
    public class IntSetting
    {
        [Tooltip("対象のMonoBehaviour")]
        public MonoBehaviour targetScript;
        [Tooltip("変更する変数名")]
        public string variableName;
        [Tooltip("設定する値")]
        public int value;
    }
    public IntSetting[] intSettings;
    [System.Serializable]
    public class FloatSetting
    {
        [Tooltip("対象のMonoBehaviour")]
        public MonoBehaviour targetScript;
        [Tooltip("変更する変数名")]
        public string variableName;
        [Tooltip("設定する値")]
        public float value;
    }
    public FloatSetting[] floatSettings;
    [System.Serializable]
    public class StringSetting
    {
        [Tooltip("対象のMonoBehaviour")]
        public MonoBehaviour targetScript;
        [Tooltip("変更する変数名")]
        public string variableName;
        [Tooltip("設定する値")]
        public string value;
    }
    public StringSetting[] stringSettings;
    [Tooltip("変数設定後に実行するイベント")]
    public UnityEvent onVariablesSet;

    private void OnEnable()
    {
        if (!isEnabled) return;

        ApplySettings();
    }

    /// <summary>
    /// 全ての設定を適用
    /// </summary>
    public void ApplySettings()
    {
        // Bool設定を適用
        if (boolSettings != null)
        {
            foreach (var setting in boolSettings)
            {
                if (setting.targetScript != null && !string.IsNullOrEmpty(setting.variableName))
                {
                    SetVariable(setting.targetScript, setting.variableName, setting.value);
                }
            }
        }

        // Int設定を適用
        if (intSettings != null)
        {
            foreach (var setting in intSettings)
            {
                if (setting.targetScript != null && !string.IsNullOrEmpty(setting.variableName))
                {
                    SetVariable(setting.targetScript, setting.variableName, setting.value);
                }
            }
        }

        // Float設定を適用
        if (floatSettings != null)
        {
            foreach (var setting in floatSettings)
            {
                if (setting.targetScript != null && !string.IsNullOrEmpty(setting.variableName))
                {
                    SetVariable(setting.targetScript, setting.variableName, setting.value);
                }
            }
        }

        // String設定を適用
        if (stringSettings != null)
        {
            foreach (var setting in stringSettings)
            {
                if (setting.targetScript != null && !string.IsNullOrEmpty(setting.variableName))
                {
                    SetVariable(setting.targetScript, setting.variableName, setting.value);
                }
            }
        }

        // イベント発火
        onVariablesSet?.Invoke();

        if (showDebugLog)
        {
            Debug.Log($"[OnEnableVariableSetter] All settings applied on {gameObject.name}");
        }
    }

    /// <summary>
    /// リフレクションで変数を設定
    /// </summary>
    private void SetVariable<T>(MonoBehaviour target, string variableName, T value)
    {
        var type = target.GetType();

        // フィールドを探す
        var field = type.GetField(variableName, 
            System.Reflection.BindingFlags.Public | 
            System.Reflection.BindingFlags.NonPublic | 
            System.Reflection.BindingFlags.Instance);

        if (field != null)
        {
            field.SetValue(target, value);
            if (showDebugLog)
            {
                Debug.Log($"[OnEnableVariableSetter] Set {target.GetType().Name}.{variableName} = {value}");
            }
            return;
        }

        // プロパティを探す
        var property = type.GetProperty(variableName,
            System.Reflection.BindingFlags.Public |
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Instance);

        if (property != null && property.CanWrite)
        {
            property.SetValue(target, value);
            if (showDebugLog)
            {
                Debug.Log($"[OnEnableVariableSetter] Set {target.GetType().Name}.{variableName} = {value}");
            }
            return;
        }

        Debug.LogWarning($"[OnEnableVariableSetter] Variable '{variableName}' not found on {target.GetType().Name}");
    }
}
