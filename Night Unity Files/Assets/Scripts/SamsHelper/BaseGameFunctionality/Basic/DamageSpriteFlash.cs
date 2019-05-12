using DG.Tweening;
using UnityEngine;

public class DamageSpriteFlash : MonoBehaviour
{
	private SpriteRenderer _sprite;

	private Tween colourTween;

	protected void Awake()
	{
		_sprite = GetComponent<SpriteRenderer>();
	}

	public void FlashSprite()
	{
		colourTween?.Complete();
		_sprite.color = Color.red;
		colourTween   = _sprite.DOColor(Color.white, 0.5f);
	}

	public void Kill()
	{
		colourTween.Complete();
	}
}