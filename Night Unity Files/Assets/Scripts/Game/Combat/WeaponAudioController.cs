using System;
using System.Collections;
using DG.Tweening;
using Game.Gear.Weapons;
using Game.Global;
using UnityEngine;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;

public class WeaponAudioController : MonoBehaviour
{
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
		WeaponType weaponType = weapon.WeaponType();
		AudioClip  clip;
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

	public void StopReload(Weapon weapon)
	{
		WeaponType weaponType = weapon.WeaponType();
		AudioClip  clip;
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
		AudioClip[] shots       = null;
		AudioClip[] casings     = null;
		WeaponType  weaponType  = weapon.WeaponType();
		float       maxVolume   = 0.8f;
		float       minDistance = 1f;
		switch (weaponType)
		{
			case WeaponType.Pistol:
				shots   = AudioClips.PistolShots;
				casings = AudioClips.PistolCasings;
				break;
			case WeaponType.Rifle:
				shots       = new[] {AudioClips.RifleShots[0]};
				casings     = AudioClips.RifleCasings;
				maxVolume   = 1f;
				minDistance = 2f;
				break;
			case WeaponType.Shotgun:
				shots       = AudioClips.ShotgunShots;
				casings     = AudioClips.ShotgunCasings;
				maxVolume   = 0.9f;
				minDistance = 1.5f;
				break;
			case WeaponType.SMG:
				shots     = AudioClips.SMGShots;
				casings   = AudioClips.SMGCasings;
				maxVolume = 0.7f;
				break;
		}

		Assert.IsNotNull(shots);
		if (shots.Length == 0) return;
		float durability = weapon.WeaponAttributes.GetDurability().CurrentValue / 50f;
		float hpfValue   = Mathf.Lerp(750f, 0f, durability);
		hpfValue = Mathf.Clamp(hpfValue, 0, 750);
		InstancedAudio instancedAudio = _audioPool.Create();
		instancedAudio.SetMinMaxDistance(minDistance, 100f);
		instancedAudio.SetHighPassCutoff(hpfValue);
		instancedAudio.Play(shots[0], Random.Range(maxVolume - 0.1f, maxVolume), Random.Range(0.8f, 1f));
		Sequence sequence = DOTween.Sequence();
		sequence.AppendInterval(0.25f);
		sequence.AppendCallback(() =>
		{
			instancedAudio = _audioPool.Create();
			instancedAudio.SetMinMaxDistance(0.5f, 15f);
			instancedAudio.Play(casings[0], 0.2f);
		});
	}

	public void PlayBrawlerSlash()
	{
		InstancedAudio instancedAudio = _audioPool.Create();
		instancedAudio.SetHighPassCutoff(1000);
		instancedAudio.Play(AudioClips.BrawlerSlash, Random.Range(0.4f, 0.5f), Random.Range(0.9f, 1f));
	}

	public void PlayTakeItem()
	{
		InstancedAudio instancedAudio = _audioPool.Create();
		instancedAudio.SetHighPassCutoff(Random.Range(800,          1200));
		instancedAudio.Play(AudioClips.TakeItem, Random.Range(0.5f, 0.6f), Random.Range(0.9f, 1f));
	}

	public void PlayNeedleFire()
	{
		_audioPool.Create().Play(AudioClips.NeedleFire, Random.Range(0.9f, 1f), Random.Range(0.9f, 1f));
	}

	public void PlayShieldHit()
	{
		_audioPool.Create().Play(AudioClips.ShieldHit, Random.Range(0.4f, 0.5f), Random.Range(0.9f, 1.1f));
	}

	public void PlayBodyHit()
	{
		_audioPool.Create().Play(AudioClips.BodyHit, Random.Range(0.4f, 0.5f), Random.Range(0.9f, 1.1f));
	}

	public void PlayPassiveSkill()
	{
		_audioPool.Create().Play(AudioClips.PassiveSkill, Random.Range(0.9f, 1f), Random.Range(0.9f, 1f));
	}

	public void PlayActiveSkill()
	{
		_audioPool.Create().Play(AudioClips.ActiveSkill, Random.Range(0.8f, 1f), Random.Range(0.75f, 1f));
	}

	public void PlaySaltTake()
	{
		_audioPool.Create().Play(AudioClips.SaltTake, Random.Range(0.8f, 1f), Random.Range(0.9f, 1f));
	}
}