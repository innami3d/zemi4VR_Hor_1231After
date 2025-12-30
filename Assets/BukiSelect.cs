using UnityEngine;

public class BukiSelect : MonoBehaviour
{
    public GameObject selectWaku;

    public GameObject Lance;
    public GameObject Sword;
    public GameObject Hammer;
    public void Select(Collider col, GameObject me)
    {
        selectWaku.transform.position = me.transform.position;

        string weaponName = me.gameObject.name;

        // すべての武器を一旦非アクティブに
        Lance.SetActive(false);
        Sword.SetActive(false);
        Hammer.SetActive(false);

        // 名前に応じて対応する武器をアクティブに
        switch (weaponName)
        {
            case "Lance":
                Lance.SetActive(true);
                break;
            case "Sword":
                Sword.SetActive(true);
                break;
            case "Hammer":
                Hammer.SetActive(true);
                break;
        }
    }
}
