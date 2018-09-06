using System.Collections;
using DG.Tweening;
using Game.Combat.Player;
using Game.Gear.Weapons;
using SamsHelper.Libraries;
using UnityEngine;
using UnityEngine.Assertions;

public class WeaponAudioController : MonoBehaviour
{
    private static AudioClip[] _lmgShots, _smgShots, _rifleShots, _pistolShots, _shotgunShots;
    private static AudioClip[] _lmgCasings, _smgCasings, _rifleCasings, _pistolCasings, _shotgunCasings;

    [SerializeField] private AudioClip[] _armourBreakClips;
    [SerializeField] private AudioClip _pistolClipIn, _pistolClipOut, _lmgClipIn, _lmgClipOut, _smgClipIn, _smgClipOut, _rifleClipIn, _rifleClipOut, _shotgunClipIn, _shotgunClipOut;
    [SerializeField] public AudioClip SpoolClip;

    private AudioPoolController _audioPool;
    private static bool _loaded;
    private AudioClip[] _dryFireClips;

    public void Awake()
    {
        LoadClips();
        _audioPool = GetComponent<AudioPoolController>();
    }

    private void LoadClips()
    {
        if (_loaded) return;
        _lmgShots = Resources.LoadAll<AudioClip>("Sounds/LMG/Shots");
        _smgShots = Resources.LoadAll<AudioClip>("Sounds/SMG/Shots");
        _rifleShots = Resources.LoadAll<AudioClip>("Sounds/Rifle/Shots");
        _pistolShots = Resources.LoadAll<AudioClip>("Sounds/Pistol/Shots");
        _shotgunShots = Resources.LoadAll<AudioClip>("Sounds/Shotgun/Shots");

        _lmgCasings = Resources.LoadAll<AudioClip>("Sounds/LMG/Casings");
        _smgCasings = Resources.LoadAll<AudioClip>("Sounds/SMG/Casings");
        _rifleCasings = Resources.LoadAll<AudioClip>("Sounds/Rifle/Casings");
        _pistolCasings = Resources.LoadAll<AudioClip>("Sounds/Pistol/Casings");
        _shotgunCasings = Resources.LoadAll<AudioClip>("Sounds/Shotgun/Casings");

        _dryFireClips = Resources.LoadAll<AudioClip>("Sounds/Dryfire");
        _loaded = true;
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
        Debug.Log("fart");
        _audioPool.PlayClip(_dryFireClips.RandomElement());
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
                shots = _pistolShots;
                casings = _pistolCasings;
                break;
            case WeaponType.Rifle:
                shots = _rifleShots;
                casings = _rifleCasings;
                break;
            case WeaponType.Shotgun:
                shots = _shotgunShots;
                casings = _shotgunCasings;
                break;
            case WeaponType.SMG:
                shots = _smgShots;
                casings = _smgCasings;
                break;
            case WeaponType.LMG:
                shots = _lmgShots;
                casings = _lmgCasings;
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
        if (!transform.parent.CompareTag("Player")) return;
        PlayerCombat.Instance.Shake(weapon.WeaponAttributes.DPS());
    }

    public void BreakArmour()
    {
        _audioPool.PlayClip(_armourBreakClips.RandomElement());
    }
}