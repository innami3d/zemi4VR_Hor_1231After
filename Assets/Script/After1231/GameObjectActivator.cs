using UnityEngine;
using System.Collections.Generic;

public class GameObjectActivator : MonoBehaviour
{
    [Tooltip("有効化/無効化するGameObjectの名前（複数指定可）")]
    public string[] targetGameObjectNames;

    private List<GameObject> targetObjects = new List<GameObject>();

    void OnEnable()
    {
        targetObjects.Clear();
        if (targetGameObjectNames == null) return;
        foreach (string name in targetGameObjectNames)
        {
            if (!string.IsNullOrEmpty(name))
            {
                GameObject obj = FindGameObjectByName(name);
                if (obj != null)
                {
                    obj.SetActive(true);
                    targetObjects.Add(obj);
                }
            }
        }
    }

    void OnDisable()
    {
        foreach (GameObject obj in targetObjects)
        {
            if (obj != null)
            {
                obj.SetActive(false);
            }
        }
    }

    // 非アクティブなGameObjectも検索できるメソッド
    private GameObject FindGameObjectByName(string name)
    {
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj.name == name && obj.hideFlags == HideFlags.None)
            {
                return obj;
            }
        }
        return null;
    }
}
