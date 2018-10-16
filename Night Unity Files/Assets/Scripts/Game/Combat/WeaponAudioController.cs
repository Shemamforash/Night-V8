using System.Collections;
using DG.Tweening;
using Game.Combat.Player;
using Game.Gear.Weapons;
using Game.Global;
using SamsHelper.Libraries;
using UnityEngine;
using UnityEngine.Assertions;

public class WeaponAudioController : MonoBehaviour
{
    
    [SerializeField] private AudioClip _pistolClipIn, _pistolClipOut, _lmgClipIn, _lmgClipOut, _smgClipIn, _smgClipOut, _rifleClipIn, _rifleClipOut, _shotgunClipIn, _shotgunClipOut;
    [SerializeField] public AudioClip SpoolClip;

    private AudioPoolController _audioPool;

    public void Awake()
    {
        _audioPool = GetComponent<AudioPoolController>();
    }

    public void Destroy()
    {
        transform.SetParent(null);
        StartCoroutine(DestroyWhenDone());
    }

    private IEnumerator DestroyWhenDone()
    {
        float duration = 1f;
        while (duration > 0f)
        {
            duration -= Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }

    public void StartReload(WeaponType weaponType)
    {
        AudioClip clip = null;
        switch (weaponType)
        {
            case WeaponType.Pistol:
                clip = _pistolClipOut;
                break;
            case WeaponType.Rifle:
                clip = _rifleClipOut;
                break;
            case WeaponType.Shotgun:
                clip = _shotgunClipOut;
                break;
            case WeaponType.SMG:
                clip = _smgClipOut;
                break;
            case WeaponType.LMG:
                clip = _lmgClipOut;
                break;
        }

        if (clip == null) return;
        _audioPool.PlayClip(clip);
    }

    public void DryFire()
    {
        _audioPool.PlayClip(AudioClips.DryFireClips.RandomElement());
    }

    public void StopReload(WeaponType weaponType)
    {
        AudioClip clip = null;
        switch (weaponType)
        {
            case WeaponType.Pistol:
                clip = _pistolClipIn;
                break;
            case WeaponType.Rifle:
                clip = _rifleClipIn;
                break;
            case WeaponType.Shotgun:
                clip = _shotgunClipIn;
                break;
            case WeaponType.SMG:
                clip = _smgClipIn;
                break;
            case WeaponType.LMG:
                clip = _lmgClipIn;
                break;
        }

        if (clip == null) return;
        _audioPool.PlayClip(clip);
    }

    public void Fire(Weapon weapon)
    {
        AudioClip[] shots = null;
        AudioClip[] casings = null;
        WeaponType weaponType = weapon.WeaponAttributes.WeaponType;
        switch (weaponType)
        {
            case WeaponType.Pistol:
                shots = AudioClips.PistolShots;
                casings = AudioClips.PistolCasings;
                break;
            case WeaponType.Rifle:
                shots = AudioClips.RifleShots;
                casings = AudioClips.RifleCasings;
                break;
            case WeaponType.Shotgun:
                shots = AudioClips.ShotgunShots;
                casings = AudioClips.ShotgunCasings;
                break;
            case WeaponType.SMG:
                shots = AudioClips.SMGShots;
                casings = AudioClips.SMGCasings;
                break;
            case WeaponType.LMG:
                shots = AudioClips.LMGShots;
                casings = AudioClips.LMGCasings;
                break;
        }

        Assert.IsNotNull(shots);
        if (shots.Length == 0) return;
        float durability = weapon.WeaponAttributes.GetDurability().CurrentValue();
        float hpfValue = -15f * durability + 750;
        hpfValue = Mathf.Clamp(hpfValue, 0, 750);
        _audioPool.PlayClip(shots.RandomElement(), 0, hpfValue);
        Sequence sequence = DOTween.Sequence();
        sequence.AppendInterval(0.25f);
        sequence.AppendCallback(() => { _audioPool.PlayClip(casings.RandomElement(), -0.6f); });
    }

    public void BreakArmour()
    {
        _audioPool.PlayClip(AudioClips.ArmourBreakClips.RandomElement());
    }
}