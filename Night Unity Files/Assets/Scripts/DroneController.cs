using SamsHelper.Libraries;
using UnityEngine;

public class DroneController : MonoBehaviour
{
    private static AudioClip[] _drones;
    private CrossFader _crossFader;
    private const float CrossFadeDuration = 5f;

    public void Awake()
    {
        if (_drones == null) _drones = Helper.LoadAllFilesFromAssetBundle<AudioClip>("drones");
        _crossFader = GetComponent<CrossFader>();
        _crossFader.SetCrossFadeDuration(CrossFadeDuration);
        _crossFader.SetMaxVolume(0.6f);
        _crossFader.CrossFade(_drones.RandomElement());
    }

    public void Update()
    {
        while (_crossFader.TimeRemaining() > CrossFadeDuration) return;
        _crossFader.CrossFade(_drones.RandomElement());
    }
}
