using Game.Global;
using SamsHelper.Libraries;
using UnityEngine;

public class DroneController : MonoBehaviour
{
    private CrossFader _crossFader;
    private const float CrossFadeDuration = 5f;

    public void Awake()
    {
        _crossFader = GetComponent<CrossFader>();
        _crossFader.SetCrossFadeDuration(CrossFadeDuration);
        _crossFader.SetMaxVolume(0.6f);
        _crossFader.StartAtRandomPosition();
        _crossFader.CrossFade(AudioClips.Drones.RandomElement());
    }

    public void Update()
    {
        while (_crossFader.TimeRemaining() > CrossFadeDuration) return;
        _crossFader.CrossFade(AudioClips.Drones.RandomElement());
    }
}