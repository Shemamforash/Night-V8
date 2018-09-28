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
    private static AudioClip[] _dryFireClips;
    private static AudioClip[] _armourBreakClips;
    [SerializeField] private AudioClip _pistolClipIn, _pistolClipOut, _lmgClipIn, _lmgClipOut, _smgClipIn, _smgClipOut, _rifleClipIn, _rifleClipOut, _shotgunClipIn, _shotgunClipOut;
    [SerializeField] public AudioClip SpoolClip;

    private AudioPoolController _audioPool;
    private static bool _loaded;

    public void Awake()
    {
        LoadClips();
        _audioPool = GetComponent<AudioPoolController>();
    }

    private void LoadClips()
    {
        if (_loaded) return;
        _lmgShots = Helper.LoadAllFilesFromAssetBundle<AudioClip>("combat/lmg/shots");
        _smgShots = Helper.LoadAllFilesFromAssetBundle<AudioClip>("combat/smg/shots");
        _rifleShots = Helper.LoadAllFilesFromAssetBundle<AudioClip>("combat/rifle/shots");
        _pistolShots = Helper.LoadAllFilesFromAssetBundle<AudioClip>("combat/pistol/shots");
        _shotgunShots = Helper.LoadAllFilesFromAssetBundle<AudioClip>("combat/shotgun/shots");

        _lmgCasings = Helper.LoadAllFilesFromAssetBundle<AudioClip>("combat/lmg/casings");
        _smgCasings = Helper.LoadAllFilesFromAssetBundle<AudioClip>("combat/smg/casings");
        _rifleCasings = Helper.LoadAllFilesFromAssetBundle<AudioClip>("combat/rifle/casings");
        _pistolCasings = Helper.LoadAllFilesFromAssetBundle<AudioClip>("combat/pistol/casings");
        _shotgunCasings = Helper.LoadAllFilesFromAssetBundle<AudioClip>("combat/shotgun/casings");

        _dryFireClips = Helper.LoadAllFilesFromAssetBundle<AudioClip>("combat/dryfire");
        _armourBreakClips = Helper.LoadAllFilesFromAssetBundle<AudioClip>("combat/armourbreak");
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
    }

    public void BreakArmour()
    {
        _audioPool.PlayClip(_armourBreakClips.RandomElement());
    }
}