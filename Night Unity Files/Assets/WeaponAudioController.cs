using Game.Gear.Weapons;
using SamsHelper.Libraries;
using UnityEngine;

public class WeaponAudioController : MonoBehaviour
{
	[SerializeField]
	private AudioClip[] _lmgClips, _smgClips, _rifleClips, _pistolClips, _shotgunClips;

	[SerializeField]
	private AudioClip _pistolClipIn, _pistolClipOut;
	private AudioSource _audioSource;

	public void Awake()
	{
		_audioSource = GetComponent<AudioSource>();
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
				break;
			case WeaponType.Shotgun:
				break;
			case WeaponType.SMG:
				break;
			case WeaponType.LMG:
				break;
		}

		if (clip == null) return;
		_audioSource.PlayOneShot(clip);
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
				break;
			case WeaponType.Shotgun:
				break;
			case WeaponType.SMG:
				break;
			case WeaponType.LMG:
				break;
		}

		if (clip == null) return;
		_audioSource.PlayOneShot(clip);
	}
	
	public void Fire(WeaponType weaponType)
	{
		AudioClip[] clips = null;
		switch (weaponType)
		{
			case WeaponType.Pistol:
				clips = _pistolClips;
				break;
			case WeaponType.Rifle:
				clips = _rifleClips;
				break;
			case WeaponType.Shotgun:
				clips = _shotgunClips;
				break;
			case WeaponType.SMG:
				clips = _smgClips;
				break;
			case WeaponType.LMG:
				clips = _lmgClips;
				break;
		}

		if (clips == null) return;
		if (clips.Length == 0) return;
		_audioSource.PlayOneShot(Helper.RandomInList(clips));
	}
}
