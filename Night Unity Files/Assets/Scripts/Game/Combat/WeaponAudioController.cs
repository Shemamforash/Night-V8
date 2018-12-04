using System;
using System.Collections;
using DG.Tweening;
using Game.Combat.Player;
using Game.Gear.Weapons;
using Game.Global;
using SamsHelper.Libraries;
using UnityEngine;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;

public class WeaponAudioController : MonoBehaviour
{
    [SerializeField] public AudioClip SpoolClip;

    private AudioPoolController _audioPool;

    public void Awake()
    {
        _audioPool = GetComponent<AudioPoolController>();
        _audioPool.SetMixerGroup("Gunshots", 1);
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

    public void StartReload(Weapon weapon)
    {
        WeaponType weaponType = weapon.WeaponAttributes.WeaponType;
        AudioClip clip;
        switch (weaponType)
        {
            case WeaponType.Pistol:
                clip = AudioClips.PistolClipOut;
                break;
            case WeaponType.Rifle:
                clip = AudioClips.RifleClipOut;
                break;
            case WeaponType.Shotgun:
                clip = AudioClips.ShotgunClipOut;
                break;
            case WeaponType.SMG:
                clip = AudioClips.SMGClipOut;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        _audioPool.Create().Play(clip, 0.6f, Random.Range(0.9f, 1f));
    }

    public void DryFire()
    {
        _audioPool.Create().Play(AudioClips.DryFireClips.RandomElement());
    }

    public void StopReload(Weapon weapon)
    {
        WeaponType weaponType = weapon.WeaponAttributes.WeaponType;
        AudioClip clip;
        switch (weaponType)
        {
            case WeaponType.Pistol:
                clip = AudioClips.PistolClipIn;
                break;
            case WeaponType.Rifle:
                clip = AudioClips.RifleClipIn;
                break;
            case WeaponType.Shotgun:
                clip = AudioClips.ShotgunClipIn;
                break;
            case WeaponType.SMG:
                clip = AudioClips.SMGClipIn;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        _audioPool.Create().Play(clip, 0.6f, Random.Range(0.9f, 1f));
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
                shots = new[] {AudioClips.RifleShots[0]};
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
        }

        Assert.IsNotNull(shots);
        if (shots.Length == 0) return;
        float durability = weapon.WeaponAttributes.GetDurability().CurrentValue();
        float hpfValue = -15f * durability + 750;
        hpfValue = Mathf.Clamp(hpfValue, 0, 750);
        InstancedAudio audio = _audioPool.Create();
        audio.SetMinMaxDistance(0.5f, 15f);
        audio.Play(shots[0], Random.Range(0.6f, 0.7f), Random.Range(0.8f, 1f), hpfValue);
        Sequence sequence = DOTween.Sequence();
        sequence.AppendInterval(0.25f);
        sequence.AppendCallback(() =>
        {
            audio = _audioPool.Create();
            audio.SetMinMaxDistance(0.5f, 15f);
            audio.Play(casings[0], 0.2f);
        });
    }

    public void BreakArmour()
    {
        _audioPool.Create().Play(AudioClips.ArmourBreakClips.RandomElement());
    }

    public void PlayBrawlerSlash()
    {
        _audioPool.Create().Play(AudioClips.BrawlerSlash, Random.Range(0.4f, 0.5f), Random.Range(0.9f, 1f), 1000);
    }

    public void PlayTakeItem()
    {
        _audioPool.Create().Play(AudioClips.TakeItem, Random.Range(0.5f, 0.6f), Random.Range(0.9f, 1f), Random.Range(800, 1200));
    }
}