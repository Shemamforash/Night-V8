using System.Collections.Generic;
using Game.Gear;
using Game.Gear.Weapons;
using Extensions;
using SamsHelper.ReactiveUI;
using TMPro;
using UnityEngine;

public class DurabilityBarController : MonoBehaviour
{
	private const float AbsoluteMaxDurability = ((int) ItemQuality.Radiant + 1) * 10;
	private const float WorldWidthCoefficient = 0.004367f;

	private readonly List<GameObject> _leftMarkers  = new List<GameObject>();
	private readonly List<GameObject> _rightMarkers = new List<GameObject>();
	private          ParticleSystem   _durabilityParticles;
	private          TextMeshProUGUI  _durabilityText;
	private          RectTransform    _durabilityTransform;
	private          bool             _forceUpdate;
	private          float            _lastPixelWidth;
	private          Weapon           _weapon;

	public void Awake()
	{
		_durabilityTransform = gameObject.FindChildWithName<RectTransform>("Max");
		_durabilityParticles = gameObject.FindChildWithName<ParticleSystem>("Current");
		_durabilityText      = transform.Find("Text")?.GetComponent<TextMeshProUGUI>();

		GameObject leftMarkerObject = gameObject.FindChildWithName("Left");
		_leftMarkers.Add(leftMarkerObject.FindChildWithName("I"));
		_leftMarkers.Add(leftMarkerObject.FindChildWithName("II"));
		_leftMarkers.Add(leftMarkerObject.FindChildWithName("III"));
		_leftMarkers.Add(leftMarkerObject.FindChildWithName("IV"));
		_leftMarkers.Add(leftMarkerObject.FindChildWithName("V"));

		GameObject rightMarkerObject = gameObject.FindChildWithName("Right");
		_rightMarkers.Add(rightMarkerObject.FindChildWithName("I"));
		_rightMarkers.Add(rightMarkerObject.FindChildWithName("II"));
		_rightMarkers.Add(rightMarkerObject.FindChildWithName("III"));
		_rightMarkers.Add(rightMarkerObject.FindChildWithName("IV"));
		_rightMarkers.Add(rightMarkerObject.FindChildWithName("V"));
	}

	public void Reset()
	{
		_durabilityTransform.anchorMin = Vector2.zero;
		_durabilityTransform.anchorMax = Vector2.zero;
		UpdateMarkers(0);
	}

	private void SetText(string str)
	{
		if (_durabilityText == null) return;
		_durabilityText.text = str;
	}

	private void UpdateMarkers(int max)
	{
		for (int i = 0; i < _leftMarkers.Count; ++i)
		{
			bool active = i < max;
			_leftMarkers[i].SetActive(active);
			_rightMarkers[i].SetActive(active);
		}
	}

	public void Update()
	{
		if (Time.timeScale < 0.1f) _durabilityParticles.Simulate(Time.unscaledDeltaTime, true, false);

		float pixelWidth = _durabilityTransform.rect.width;
		if (!_durabilityParticles.isPlaying) _durabilityParticles.Play();
		if (!_forceUpdate && pixelWidth == _lastPixelWidth) return;

		float currentLevel = _weapon.WeaponAttributes.CurrentLevel;
		float maxLevel        = _weapon.WeaponAttributes.MaxLevel;
		float normalisedLevel = currentLevel / maxLevel;
		float rectAnchorOffset     = maxLevel / AbsoluteMaxDurability / 2;
		SetText((int) currentLevel + " Imbued Essence");

		_durabilityParticles.Clear();
		_forceUpdate    = false;
		_lastPixelWidth = pixelWidth;

		float yAnchor = _durabilityTransform.anchorMin.y;
		_durabilityTransform.anchorMin = new Vector2(0.5f - rectAnchorOffset, yAnchor);
		_durabilityTransform.anchorMax = new Vector2(0.5f + rectAnchorOffset, yAnchor);

		float                         worldWidth = pixelWidth * WorldWidthCoefficient * normalisedLevel;
		ParticleSystem.ShapeModule    shape      = _durabilityParticles.shape;
		ParticleSystem.EmissionModule emission   = _durabilityParticles.emission;
		shape.radius          = worldWidth;
		emission.rateOverTime = 300 * worldWidth / 3;
	}

	public void SetWeapon(Weapon weapon)
	{
		_weapon      = weapon;
		_forceUpdate = true;
		_durabilityParticles.Clear();
		UpdateMarkers(_weapon.WeaponAttributes.MaxLevel / 10);
	}
}