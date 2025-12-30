using UnityEngine;
using Oculus.Haptics;
using System;

public class HapticsPlayer : MonoBehaviour
{
    [SerializeField] HapticClip _hapticClip;
    [SerializeField] HapticClipPlayer _hapticClipPlayer;

    protected virtual void Start()
    {
        _hapticClipPlayer = new HapticClipPlayer(_hapticClip);
    }
    protected virtual void OnDestroy()
    {
        _hapticClipPlayer.Dispose();
    }
    protected virtual void OnApplicationQuit()
    {
        Haptics.Instance.Dispose();
    }

    public void RightHap()
    {
        _hapticClipPlayer.Play(Controller.Right);
    }
    public void LeftHap()
    {
        _hapticClipPlayer.Play(Controller.Left);
    }
}
