using Facilitating.Audio;
using Game.Combat.Enemies.Bosses;
using Game.Combat.Misc;
using Game.Exploration.Environment;
using Game.Global;
using Extensions;
using UnityEngine;

public class TombPortalBehaviour : CanTakeDamage
{
	private static AudioPoolController _audioPool;

	protected override void Awake()
	{
		_audioPool = GetComponent<AudioPoolController>();
		_audioPool.SetMixerGroup("Combat", 0.5f);
		SpriteFlash      = gameObject.FindChildWithName<DamageSpriteFlash>("Shadow 5");
		_bloodSpatter    = GetComponent<BloodSpatter>();
		gameObject.layer = 24;
		int health = ((int) EnvironmentManager.CurrentEnvironmentType + 1) * 200;
		HealthController.SetInitialHealth(health, this);
	}

	public override void Kill()
	{
		CreateBoss();
		base.Kill();
	}

	public override void TakeShotDamage(Shot shot)
	{
		base.TakeShotDamage(shot);
		InstancedAudio instancedAudio = _audioPool.Create();
		instancedAudio.SetMinMaxDistance(1, 100);
		instancedAudio.Play(AudioClips.TombHits.RandomElement(), Random.Range(0.9f, 1f), Random.Range(0.9f, 1f));
	}

	public override string GetDisplayName() => "Sealed Tomb";

	private void CreateBoss()
	{
		InstancedAudio instancedAudio = _audioPool.Create(false);
		instancedAudio.SetMinMaxDistance(1, 100);
		instancedAudio.Play(AudioClips.TombBreak, Random.Range(0.6f, 7f), Random.Range(0.9f, 1f));
		ThunderController.Instance().Strike(true);
		switch (EnvironmentManager.CurrentEnvironmentType)
		{
			case EnvironmentType.Desert:
				SerpentBehaviour.Create();
				break;
			case EnvironmentType.Mountains:
				StarfishBehaviour.Create();
				break;
			case EnvironmentType.Sea:
				SwarmBehaviour.Create();
				break;
			case EnvironmentType.Ruins:
				OvaBehaviour.Create();
				break;
		}
	}
}