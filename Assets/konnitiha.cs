using UnityEngine;
using TMPro;

public class konnitiha : MonoBehaviour
{
    int count = 0;
    public SendOSC sendOSC;

    public TextMeshPro text;
    public void inputAisatu(string aisatu)
    {
        if (count == 0 && aisatu == "こ")
        {
            count++;
            text.text = "こ";
            sendOSC.SendOsc("/event/Aisatu:こ");
        }
        else if (count == 1 && aisatu == "ん")
        {
            count++;
            text.text = "こん";
            sendOSC.SendOsc("/event/Aisatu:ん");
        }
        else if (count == 2 && aisatu == "に")
        {
            count++;
            text.text = "こんに";
            sendOSC.SendOsc("/event/Aisatu:に");
        }
        else if (count == 3 && aisatu == "ち")
        {
            count++;
            text.text = "こんにち";
            sendOSC.SendOsc("/event/Aisatu:ち");
        }
        else if (count == 4 && aisatu == "は")
        {
            count++;
            text.text = "こんにちは";
            sendOSC.SendOsc("/event/Aisatu:は");
            sendOSC.SendOsc("/cue/call/Aisatu");
        }
    }
}
