using System;
using DG.Tweening;
using Extensions;
using Game.Global;

using SamsHelper.Libraries;
using UnityEngine;

public class RescuePuzzleButtonController : MonoBehaviour
{
	private AudioSource    _audioSource;
	private bool           _disabled;
	private bool           _fading;
	private SpriteRenderer _image, _glow;
	private Action         _onPress;

	private void Awake()
	{
		_audioSource = GetComponent<AudioSource>();
		_image       = gameObject.FindChildWithName<SpriteRenderer>("Select");
		_glow        = gameObject.FindChildWithName<SpriteRenderer>("Glow");
		EnableButton();
	}

	private void OnCollisionEnter2D(Collision2D other)
	{
		if (_disabled) return;
		_fading = true;
		_glow.SetAlpha(1f);
		_image.SetAlpha(1f);
		_glow.DOFade(0f, 1.5f);
		_image.DOFade(0.25f, 1.5f);
		_audioSource.clip = AudioClips.Chimes.RandomElement();
		_audioSource.Play();
		_onPress();
	}

	public void SetOnPress(Action onPress)
	{
		_onPress = onPress;
	}

	public void DisableButton()
	{
		_disabled = true;
		if (_fading) return;
		_image.DOFade(0.25f, 0.5f);
		_glow.DOFade(0f, 1.5f);
	}

	public void EnableButton()
	{
		_fading   = false;
		_disabled = false;
		_image.SetAlpha(0.75f);
		_glow.SetAlpha(0f);
	}
}