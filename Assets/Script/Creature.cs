using UnityEngine;

public class Creature : MonoBehaviour
{
    public ParticleSystem ps;
    bool isAttack = false;
    public SendOSC sendOSC;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "Knife" && isAttack)
        {
            ps.Play();
            ps.gameObject.transform.position = other.transform.position;
            sendOSC.SendOsc("/event/attackBoss");
            isAttack = false;
        }
    }
    public void SetBool(bool b)
    {
        isAttack = b;
    }
}
