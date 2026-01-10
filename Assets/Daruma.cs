using UnityEngine;

public class Daruma : MonoBehaviour
{
    [Header("対象オブジェクト（マテリアル変更用）")]
    public GameObject gameObject1;
    public GameObject gameObject2;

    [Header("表示切替用オブジェクト")]
    public GameObject waitObject;
    public GameObject korondaObject;

    /// <summary>
    /// マテリアルを赤にする、Waitをアクティブにする
    /// </summary>
    public void OnHitWait()
    {
        SetMaterialColor(Color.red);

        if (waitObject != null) waitObject.SetActive(true);
        if (korondaObject != null) korondaObject.SetActive(false);
    }

    /// <summary>
    /// マテリアルを青にする、Korondaをアクティブにする
    /// </summary>
    public void OnHitKoronda()
    {
        SetMaterialColor(Color.blue);

        if (waitObject != null) waitObject.SetActive(false);
        if (korondaObject != null) korondaObject.SetActive(true);
    }

    private void SetMaterialColor(Color color)
    {
        if (gameObject1 != null)
        {
            Renderer renderer1 = gameObject1.GetComponent<Renderer>();
            if (renderer1 != null)
            {
                renderer1.material.color = color;
            }
        }

        if (gameObject2 != null)
        {
            Renderer renderer2 = gameObject2.GetComponent<Renderer>();
            if (renderer2 != null)
            {
                renderer2.material.color = color;
            }
        }
    }
}
