using DG.Tweening;
using Extensions;
using SamsHelper.Libraries;
using UnityEngine;

public class BossRingController : MonoBehaviour
{
	private static GameObject _bossPrefab;

	private void Awake()
	{
		AddBossRing("Ring 5", 0.9f);
		AddBossRing("Ring 4", 0.85f);
		AddBossRing("Ring 3", 0.8f);
		AddBossRing("Ring 2", 0.75f);
		AddBossRing("Ring 1", 0.7f);
	}

	public static void Create()
	{
		if (_bossPrefab == null) _bossPrefab = Resources.Load<GameObject>("Prefabs/Combat/Buildings/Boss Rings");
		Instantiate(_bossPrefab, Vector2.zero, Quaternion.identity);
	}

	private void AddBossRing(string ringName, float pitch)
	{
		GameObject ring     = gameObject.FindChildWithName(ringName);
		BossRing   bossRing = ring.AddComponent<BossRing>();
		bossRing.SetPitch(pitch);
	}

	private class BossRing : MonoBehaviour
	{
		private AudioSource    _audioSource;
		private SpriteRenderer _ring, _glow;

		public void Awake()
		{
			_ring = GetComponent<SpriteRenderer>();
			_glow = _ring.gameObject.FindChildWithName<SpriteRenderer>("Glow");
			_ring.SetAlpha(0.2f);
			_glow.SetAlpha(0f);
			_audioSource = _ring.GetComponent<AudioSource>();
		}

		public void SetPitch(float pitch) => _audioSource.pitch = pitch;

		public void OnTriggerEnter2D(Collider2D other)
		{
			_audioSource.Play();
			_glow.SetAlpha(1f);
			_glow.DOFade(0f, 1f);
			_ring.SetAlpha(0.8f);
			_ring.DOFade(0.2f, 2f);
			Destroy(GetComponent<Collider2D>());
		}
	}
}