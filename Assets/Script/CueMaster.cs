using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using OscJack;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// VRゲームのCue進行を管理するMasterスクリプト
/// Cue番号の変更時にInspectorで設定したイベントに通知します
/// </summary>
public class CueMaster : MonoBehaviour
{
    [Header("Screen Reference")]
    [Tooltip("ScreenのGameObject参照")]
    [SerializeField] private GameObject screen;

    [Header("Cue Settings")]
    [Tooltip("現在のCue番号")]
    [SerializeField] private int currentCue = 0;

    [Tooltip("スタート時にCue 0を自動再生")]
    public bool playOnStart = true;

    [Tooltip("デバッグログを表示")]
    public bool showDebugLog = true;

    [Header("Global Events（すべてのCue変更時に発火）")]
    [Tooltip("Cueが変更されたときに発火するイベント（新しいCue番号を渡します）")]
    public UnityEvent<int> OnCueChanged;

    [Tooltip("Cueが変更されたときに発火するイベント（引数なし）")]
    public UnityEvent OnCueChangedTrigger;

    /// <summary>
    /// Cue番号ごとのイベントマッピング
    /// </summary>
    [System.Serializable]
    public class CueEvent
    {
        [Tooltip("このイベントが発火するCue番号")]
        public int cueNumber;

        [Tooltip("このCue番号になったときに発火するイベント")]
        public UnityEvent onCueTriggered;
    }

    [Header("Cue-Specific Events（特定のCue番号で発火）")]
    [Tooltip("Cue番号ごとのイベントマッピング")]
    public List<CueEvent> cueEvents = new List<CueEvent>();

    [Header("Development Skip（開発用スキップ）")]
    [Tooltip("開発時に途中から開始するためのターゲットCue番号")]
    public int skipTargetCue = 0;

    [Tooltip("各Cue間のスキップ間隔（秒）")]
    [SerializeField] private float skipInterval = 0.5f;

    // スキップ中かどうか
    private bool isSkipping = false;
    private Coroutine skipCoroutine;

    /// <summary>
    /// 現在のCue番号を取得
    /// </summary>
    public int CurrentCue => currentCue;

    /// <summary>
    /// スキップ中かどうかを取得
    /// </summary>
    public bool IsSkipping => isSkipping;
    private void Start()
    {
        if (playOnStart)
        {
            if (showDebugLog)
            {
                Debug.Log("[CueMaster] Auto-playing Cue 0 on Start");
            }
            NotifyCueChanged();
        }
    }

    /// <summary>
    /// 指定したCue番号に設定し、イベントを発火
    /// </summary>
    /// <param name="cueNumber">設定するCue番号</param>
    public void SetCue(int cueNumber)
    {
        int previousCue = currentCue;
        currentCue = cueNumber;

        if (showDebugLog)
        {
            Debug.Log($"[CueMaster] Cue changed: {previousCue} -> {currentCue}");
        }

        NotifyCueChanged();
    }

    /// <summary>
    /// Cue番号を+1して次に進める
    /// </summary>
    public void NextCue()
    {
        SetCue(currentCue + 1);
    }

    /// <summary>
    /// Cue番号を-1して前に戻す
    /// </summary>
    public void PreviousCue()
    {
        SetCue(currentCue - 1);
    }

    /// <summary>
    /// Cue番号を0にリセット
    /// </summary>
    public void ResetCue()
    {
        SetCue(0);
    }

    /// <summary>
    /// 指定したCue番号まで0.5秒間隔でキューを進める（開発用）
    /// </summary>
    /// <param name="targetCue">目標のCue番号</param>
    public void SkipToCue(int targetCue)
    {
        if (isSkipping)
        {
            if (showDebugLog)
            {
                Debug.LogWarning("[CueMaster] Already skipping!");
            }
            return;
        }

        if (targetCue <= currentCue)
        {
            if (showDebugLog)
            {
                Debug.LogWarning($"[CueMaster] Target cue {targetCue} is not greater than current cue {currentCue}");
            }
            return;
        }

        skipCoroutine = StartCoroutine(SkipToCueCoroutine(targetCue));
    }

    /// <summary>
    /// Inspectorで設定したターゲットCueまでスキップ（開発用）
    /// </summary>
    public void SkipToTargetCue()
    {
        SkipToCue(skipTargetCue);
    }

    /// <summary>
    /// スキップを中断
    /// </summary>
    public void StopSkip()
    {
        if (skipCoroutine != null)
        {
            StopCoroutine(skipCoroutine);
            skipCoroutine = null;
        }
        isSkipping = false;

        if (showDebugLog)
        {
            Debug.Log($"[CueMaster] Skip stopped at Cue {currentCue}");
        }
    }

    /// <summary>
    /// スキップ用のコルーチン
    /// </summary>
    private IEnumerator SkipToCueCoroutine(int targetCue)
    {
        isSkipping = true;

        if (showDebugLog)
        {
            Debug.Log($"[CueMaster] Starting skip from Cue {currentCue} to Cue {targetCue}");
        }

        while (currentCue < targetCue)
        {
            yield return new WaitForSeconds(skipInterval);
            NextCue();
        }

        isSkipping = false;
        skipCoroutine = null;

        if (showDebugLog)
        {
            Debug.Log($"[CueMaster] Skip completed at Cue {currentCue}");
        }
    }

    /// <summary>
    /// Cue変更イベントを通知
    /// </summary>
    private void NotifyCueChanged()
    {
        // グローバルイベントを発火
        OnCueChanged?.Invoke(currentCue);
        OnCueChangedTrigger?.Invoke();

        // 該当するCue番号のイベントを発火
        foreach (var cueEvent in cueEvents)
        {
            if (cueEvent.cueNumber == currentCue)
            {
                cueEvent.onCueTriggered?.Invoke();

                if (showDebugLog)
                {
                    Debug.Log($"[CueMaster] Cue {currentCue} event triggered");
                }
            }
        }
    }

    /// <summary>
    /// Screenへの参照を設定
    /// </summary>
    public void SetScreen(GameObject screenObject) => screen = screenObject;

    /// <summary>
    /// ScreenのGameObjectを取得
    /// </summary>
    public GameObject GetScreen() => screen;

    /// <summary>
    /// ScreenへY軸のみでLookAtし、4つの回転候補から最も近いものを選ぶ
    /// </summary>
    public void LookAtScreenWithSnap()
    {
        if (screen == null)
        {
            if (showDebugLog)
            {
                Debug.LogWarning("[CueMaster] Screen is not set!");
            }
            return;
        }

        // 現在のY軸回転を取得
        float currentYRotation = transform.eulerAngles.y;

        // 4つの選択肢を作成（現在のY回転、+90、+180、+270）
        float[] rotationOptions = new float[]
        {
            NormalizeAngle(currentYRotation),
            NormalizeAngle(currentYRotation + 90f),
            NormalizeAngle(currentYRotation + 180f),
            NormalizeAngle(currentYRotation + 270f)
        };

        // Screenへの方向を計算（Y軸のみ）
        Vector3 directionToScreen = screen.transform.position - transform.position;
        directionToScreen.y = 0; // Y軸成分を無視

        if (directionToScreen.sqrMagnitude < 0.001f)
        {
            if (showDebugLog)
            {
                Debug.LogWarning("[CueMaster] Screen is too close to calculate direction!");
            }
            return;
        }

        // ScreenへのY軸回転を計算
        float targetYRotation = Mathf.Atan2(directionToScreen.x, directionToScreen.z) * Mathf.Rad2Deg;
        targetYRotation = NormalizeAngle(targetYRotation);

        // 4つの候補から最も近い回転を選ぶ
        float closestRotation = rotationOptions[0];
        float minDifference = Mathf.Abs(Mathf.DeltaAngle(targetYRotation, rotationOptions[0]));

        for (int i = 1; i < rotationOptions.Length; i++)
        {
            float difference = Mathf.Abs(Mathf.DeltaAngle(targetYRotation, rotationOptions[i]));
            if (difference < minDifference)
            {
                minDifference = difference;
                closestRotation = rotationOptions[i];
            }
        }

        // 回転を適用
        Vector3 newRotation = transform.eulerAngles;
        newRotation.y = closestRotation;
        transform.eulerAngles = newRotation;

        if (showDebugLog)
        {
            Debug.Log($"[CueMaster] LookAtScreen: Target={targetYRotation:F1}°, Selected={closestRotation:F1}° (from options: {rotationOptions[0]:F1}, {rotationOptions[1]:F1}, {rotationOptions[2]:F1}, {rotationOptions[3]:F1})");
        }
    }

    /// <summary>
    /// 角度を0-360の範囲に正規化
    /// </summary>
    private float NormalizeAngle(float angle)
    {
        while (angle < 0f) angle += 360f;
        while (angle >= 360f) angle -= 360f;
        return angle;
    }
}

// --- Inspector拡張 ---
#if UNITY_EDITOR
[CustomEditor(typeof(CueMaster))]
public class CueMasterEditor : Editor
{
    private UnityEditorInternal.ReorderableList reorderableList;
    private SerializedProperty cueEventsProperty;

    private void OnEnable()
    {
        cueEventsProperty = serializedObject.FindProperty("cueEvents");
        
        reorderableList = new UnityEditorInternal.ReorderableList(
            serializedObject,
            cueEventsProperty,
            true,  // draggable
            true,  // displayHeader
            true,  // displayAddButton
            true   // displayRemoveButton
        );

        reorderableList.drawHeaderCallback = (Rect rect) =>
        {
            EditorGUI.LabelField(rect, "Cue Events（ドラッグ＆ドロップで並び替え可能）");
        };

        reorderableList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            SerializedProperty element = cueEventsProperty.GetArrayElementAtIndex(index);
            SerializedProperty cueNumberProp = element.FindPropertyRelative("cueNumber");
            SerializedProperty eventProp = element.FindPropertyRelative("onCueTriggered");

            rect.y += 2;
            float lineHeight = EditorGUIUtility.singleLineHeight;
            
            // Cue番号ラベルと入力フィールド
            Rect labelRect = new Rect(rect.x, rect.y, 50, lineHeight);
            Rect numberRect = new Rect(rect.x + 55, rect.y, 50, lineHeight);
            
            EditorGUI.LabelField(labelRect, $"Cue", EditorStyles.boldLabel);
            EditorGUI.PropertyField(numberRect, cueNumberProp, GUIContent.none);
            
            // イベントフィールド（次の行から）
            Rect eventRect = new Rect(rect.x, rect.y + lineHeight + 4, rect.width, EditorGUI.GetPropertyHeight(eventProp));
            EditorGUI.PropertyField(eventRect, eventProp, GUIContent.none);
        };

        reorderableList.elementHeightCallback = (int index) =>
        {
            SerializedProperty element = cueEventsProperty.GetArrayElementAtIndex(index);
            SerializedProperty eventProp = element.FindPropertyRelative("onCueTriggered");
            return EditorGUIUtility.singleLineHeight + EditorGUI.GetPropertyHeight(eventProp) + 10;
        };

        reorderableList.onAddCallback = (UnityEditorInternal.ReorderableList list) =>
        {
            int index = list.serializedProperty.arraySize;
            list.serializedProperty.arraySize++;
            list.index = index;

            SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex(index);
            
            // 次のCue番号を自動設定
            int nextCue = 0;
            if (index > 0)
            {
                SerializedProperty prevElement = list.serializedProperty.GetArrayElementAtIndex(index - 1);
                nextCue = prevElement.FindPropertyRelative("cueNumber").intValue + 1;
            }
            element.FindPropertyRelative("cueNumber").intValue = nextCue;
            
            // イベントをクリア
            SerializedProperty eventCalls = element.FindPropertyRelative("onCueTriggered.m_PersistentCalls.m_Calls");
            if (eventCalls != null)
            {
                eventCalls.arraySize = 0;
            }
        };
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        CueMaster cueMaster = (CueMaster)target;

        // Cue Settings
        EditorGUILayout.LabelField("Cue Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("currentCue"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("playOnStart"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("showDebugLog"));

        EditorGUILayout.Space();

        // Global Events
        EditorGUILayout.LabelField("Global Events（すべてのCue変更時に発火）", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("OnCueChanged"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("OnCueChangedTrigger"));

        EditorGUILayout.Space();

        // Cue-Specific Events (ReorderableList)
        EditorGUILayout.LabelField("Cue-Specific Events（特定のCue番号で発火）", EditorStyles.boldLabel);
        reorderableList.DoLayoutList();

        EditorGUILayout.Space();

        // Development Skip
        EditorGUILayout.LabelField("Development Skip（開発用スキップ）", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("skipTargetCue"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("skipInterval"));

        EditorGUILayout.Space();

        // Runtime Controls (プレイモードのみ)
        if (Application.isPlaying)
        {
            EditorGUILayout.LabelField("Runtime Controls", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("◀ Previous", GUILayout.Height(30)))
            {
                cueMaster.PreviousCue();
            }
            
            GUIStyle centerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 16
            };
            EditorGUILayout.LabelField($"Cue: {cueMaster.CurrentCue}", centerStyle, GUILayout.Height(30));
            
            if (GUILayout.Button("Next ▶", GUILayout.Height(30)))
            {
                cueMaster.NextCue();
            }
            
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            if (GUILayout.Button("Reset (Cue 0)", GUILayout.Height(25)))
            {
                cueMaster.ResetCue();
            }
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif

