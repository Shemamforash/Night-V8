using EZCameraShake;
using Game.Combat.Player;
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
	private AudioHighPassFilter _highPassFilter;

	public void Awake()
	{
		_audioSource = GetComponent<AudioSource>();
		_highPassFilter = GetComponent<AudioHighPassFilter>();
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
	
	public void Fire(Weapon weapon)
	{
		AudioClip[] clips = null;
		WeaponType weaponType = weapon.WeaponAttributes.WeaponType;
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
		float durability = weapon.WeaponAttributes.GetDurability().CurrentValue();
		float hpfValue = -15f * durability + 750;
		hpfValue = Mathf.Clamp(hpfValue, 0, 750);
		_highPassFilter.cutoffFrequency = hpfValue;
		_audioSource.PlayOneShot(Helper.RandomElement(clips));
		if (!transform.parent.CompareTag("Player")) return;
		PlayerCombat.Instance.Shake(weapon.WeaponAttributes.DPS());
	}
}
