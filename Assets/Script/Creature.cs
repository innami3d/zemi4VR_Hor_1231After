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

    private void OnHit(Collider other, GameObject me)
    {
        if (other.gameObject.name == "Knife" && isAttack)
        {
            sendOSC.SendOsc("/cue/call/KillCreature");
            isAttack = false;
        }
    }
}
