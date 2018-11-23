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

    public void StartReload()
    {
        _audioPool.Create().Play(AudioClips.ClipOut);
    }

    public void DryFire()
    {
        _audioPool.Create().Play(AudioClips.DryFireClips.RandomElement());
    }

    public void StopReload()
    {
        _audioPool.Create().Play(AudioClips.ClipIn);
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
        _audioPool.Create().Play(shots[0], 1f, Random.Range(0.9f, 1f), hpfValue);
        Sequence sequence = DOTween.Sequence();
        sequence.AppendInterval(0.25f);
        sequence.AppendCallback(() => { _audioPool.Create().Play(casings[0], -0.6f); });
    }

    public void BreakArmour()
    {
        _audioPool.Create().Play(AudioClips.ArmourBreakClips.RandomElement());
    }

    public void AddRound()
    {
        _audioPool.Create().Play(AudioClips.BulletLoad.RandomElement());
    }

    public void PlayBrawlerSlash()
    {
        _audioPool.Create().Play(AudioClips.BrawlerSlash, Random.Range(0.4f, 0.5f), Random.Range(0.9f, 1f), 1000);
    }
}