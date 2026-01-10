using UnityEngine;
using TMPro;
using System.Collections;

public class TextType : MonoBehaviour
{
    public TextMeshPro tmp;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    // Update is called once per frame
    void Start()
    {
        Type(gameObject.name);
    }

    public void Type(string target)
    {
        StartCoroutine(_type(target));
    }
    IEnumerator _type(string target)
    {
        string bases = "";
        for (int i = 0; i < target.Length; i++)
        {
            bases += target[i];
            tmp.text = bases;
            yield return new WaitForSeconds(0.03f);
        }
    }
}
