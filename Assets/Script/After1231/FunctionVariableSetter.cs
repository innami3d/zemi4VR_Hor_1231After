using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 関数呼び出しで指定したスクリプトの変数を変更する（UnityEvent対応版）
/// OnEnableVariableSetterの関数版 - UnityEventから呼び出し可能
/// 変数名はプルダウンから選択可能
/// どの型でもフレキシブルに設定可能
/// </summary>
public class FunctionVariableSetter : MonoBehaviour
{
    [Tooltip("デバッグログを表示")]
    public bool showDebugLog = false;

    /// <summary>
    /// 汎用設定クラス - どの型でもstring経由で設定可能
    /// </summary>
    [System.Serializable]
    public class GenericSetting
    {
        [Tooltip("対象のMonoBehaviour")]
        public MonoBehaviour targetScript;
        [Tooltip("変更する変数名")]
        public string variableName;
        [Tooltip("設定する値（文字列で入力、自動変換される）")]
        public string valueString;

        [Header("オブジェクト参照用")]
        [Tooltip("GameObject参照を設定する場合")]
        public GameObject gameObjectValue;
        [Tooltip("Transform参照を設定する場合")]
        public Transform transformValue;
        [Tooltip("コンポーネント参照を設定する場合")]
        public Component componentValue;
    }
    public GenericSetting[] genericSettings;

    [System.Serializable]
    public class BoolSetting
    {
        [Tooltip("対象のMonoBehaviour")]
        public MonoBehaviour targetScript;
        [Tooltip("変更する変数名（プルダウンから選択）")]
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
        [Tooltip("変更する変数名（プルダウンから選択）")]
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
        [Tooltip("変更する変数名（プルダウンから選択）")]
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
        [Tooltip("変更する変数名（プルダウンから選択）")]
        public string variableName;
        [Tooltip("設定する値")]
        public string value;
    }
    public StringSetting[] stringSettings;

    [System.Serializable]
    public class Vector3Setting
    {
        [Tooltip("対象のMonoBehaviour")]
        public MonoBehaviour targetScript;
        [Tooltip("変更する変数名")]
        public string variableName;
        [Tooltip("設定する値")]
        public Vector3 value;
    }
    public Vector3Setting[] vector3Settings;

    [System.Serializable]
    public class ColorSetting
    {
        [Tooltip("対象のMonoBehaviour")]
        public MonoBehaviour targetScript;
        [Tooltip("変更する変数名")]
        public string variableName;
        [Tooltip("設定する値")]
        public Color value = Color.white;
    }
    public ColorSetting[] colorSettings;

    [Tooltip("変数設定後に実行するイベント")]
    public UnityEvent onVariablesSet;

    private void OnEnable()
    {
        ApplySettings();
    }

    /// <summary>
    /// 全ての設定を適用（UnityEventから呼び出し可能）
    /// </summary>
    public void ApplySettings()
    {
        // Generic設定を適用
        if (genericSettings != null)
        {
            foreach (var setting in genericSettings)
            {
                if (setting.targetScript != null && !string.IsNullOrEmpty(setting.variableName))
                {
                    ApplyGenericSetting(setting);
                }
            }
        }

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

        // Vector3設定を適用
        if (vector3Settings != null)
        {
            foreach (var setting in vector3Settings)
            {
                if (setting.targetScript != null && !string.IsNullOrEmpty(setting.variableName))
                {
                    SetVariable(setting.targetScript, setting.variableName, setting.value);
                }
            }
        }

        // Color設定を適用
        if (colorSettings != null)
        {
            foreach (var setting in colorSettings)
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
            Debug.Log($"[FunctionVariableSetter] All settings applied on {gameObject.name}");
        }
    }

    /// <summary>
    /// 汎用設定を適用（変数の型を自動判別）
    /// </summary>
    private void ApplyGenericSetting(GenericSetting setting)
    {
        var type = setting.targetScript.GetType();
        var flags = System.Reflection.BindingFlags.Public |
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Instance;

        // フィールドを探す
        var field = type.GetField(setting.variableName, flags);
        if (field != null)
        {
            object value = ConvertValue(field.FieldType, setting);
            if (value != null)
            {
                field.SetValue(setting.targetScript, value);
                if (showDebugLog)
                    Debug.Log($"[FunctionVariableSetter] Set {type.Name}.{setting.variableName} = {value}");
            }
            return;
        }

        // プロパティを探す
        var property = type.GetProperty(setting.variableName, flags);
        if (property != null && property.CanWrite)
        {
            object value = ConvertValue(property.PropertyType, setting);
            if (value != null)
            {
                property.SetValue(setting.targetScript, value);
                if (showDebugLog)
                    Debug.Log($"[FunctionVariableSetter] Set {type.Name}.{setting.variableName} = {value}");
            }
            return;
        }

        Debug.LogWarning($"[FunctionVariableSetter] Variable '{setting.variableName}' not found on {type.Name}");
    }

    /// <summary>
    /// 設定値を適切な型に変換
    /// </summary>
    private object ConvertValue(System.Type targetType, GenericSetting setting)
    {
        // オブジェクト参照の場合
        if (targetType == typeof(GameObject) && setting.gameObjectValue != null)
            return setting.gameObjectValue;
        if (targetType == typeof(Transform) && setting.transformValue != null)
            return setting.transformValue;
        if (typeof(Component).IsAssignableFrom(targetType) && setting.componentValue != null)
            return setting.componentValue;

        // 文字列から変換
        string valueStr = setting.valueString;
        if (string.IsNullOrEmpty(valueStr)) return null;

        try
        {
            if (targetType == typeof(bool))
                return bool.Parse(valueStr);
            if (targetType == typeof(int))
                return int.Parse(valueStr);
            if (targetType == typeof(float))
                return float.Parse(valueStr);
            if (targetType == typeof(double))
                return double.Parse(valueStr);
            if (targetType == typeof(string))
                return valueStr;
            if (targetType == typeof(Vector2))
            {
                var parts = valueStr.Replace("(", "").Replace(")", "").Split(',');
                return new Vector2(float.Parse(parts[0].Trim()), float.Parse(parts[1].Trim()));
            }
            if (targetType == typeof(Vector3))
            {
                var parts = valueStr.Replace("(", "").Replace(")", "").Split(',');
                return new Vector3(float.Parse(parts[0].Trim()), float.Parse(parts[1].Trim()), float.Parse(parts[2].Trim()));
            }
            if (targetType.IsEnum)
                return System.Enum.Parse(targetType, valueStr);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[FunctionVariableSetter] Failed to convert '{valueStr}' to {targetType.Name}: {e.Message}");
        }

        return null;
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
                Debug.Log($"[FunctionVariableSetter] Set {target.GetType().Name}.{variableName} = {value}");
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
                Debug.Log($"[FunctionVariableSetter] Set {target.GetType().Name}.{variableName} = {value}");
            }
            return;
        }

        Debug.LogWarning($"[FunctionVariableSetter] Variable '{variableName}' not found on {target.GetType().Name}");
    }
}
