using DG.Tweening;
using Extensions;
using SamsHelper.Libraries;
using UnityEngine;

public class CacheRingController : MonoBehaviour
{
	private SpriteRenderer _ring, _glow;

	private void Awake()
	{
		_ring = gameObject.GetComponent<SpriteRenderer>();
		_glow = gameObject.FindChildWithName<SpriteRenderer>("Glow");
		_ring.SetAlpha(0f);
		_glow.SetAlpha(0f);
	}

	public void SetActive(bool active)
	{
		float target = active ? 0.2f : 0f;
		_ring.DOFade(target, 1f);
	}

	public void Activate()
	{
		_ring.SetAlpha(1f);
		_ring.DOFade(0.25f, 1f);
		_glow.SetAlpha(1f);
		_glow.DOFade(0.25f, 2f);
	}

	public void Deactivate()
	{
		_glow.DOFade(0f, 1f);
		_ring.DOFade(0.1f, 3f);
		Rotate rotate = GetComponent<Rotate>();
		DOTween.To(() => rotate.RotateSpeed, f => rotate.RotateSpeed = f, 0f, 5f);
	}
}