using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Extensions;
using Facilitating.UIControllers;

using SamsHelper.Libraries;
using UnityEngine;
using Random = UnityEngine.Random;

public class UiCompassController : MonoBehaviour
{
	private const    float                            ShowItemTimeBase  = 8f;
	private const    float                            MaxDetectDistance = 10f;
	private static   UiCompassController              _instance;
	private static   List<CompassItem>                _compassItems;
	private readonly List<Tuple<float, float, float>> _alphaRotations = new List<Tuple<float, float, float>>();
	private readonly List<CompassIndicatorBehaviour>  _indicators     = new List<CompassIndicatorBehaviour>();
	private          AudioSource                      _audioSource;
	private          ParticleSystem                   _compassPulse;
	private          float                            _showItemTimeCurrent;
	private          float                            ShowItemTimeMax;

	public void Awake()
	{
		_compassPulse = gameObject.FindChildWithName<ParticleSystem>("Compass Pulse");
		_instance     = this;
		_audioSource  = GetComponent<AudioSource>();
		_compassItems = new List<CompassItem>();
		for (int i = 0; i < 18; ++i) _indicators.Add(CompassIndicatorBehaviour.Create(transform));
	}

	public static bool EmitPulse(int compassBonus)
	{
		if (_instance._compassPulse.particleCount != 0) return false;
		_instance.ShowItemTimeMax = ShowItemTimeBase + compassBonus;
		_instance._compassPulse.Play();
		_instance.StartCoroutine(_instance.ShowItems());
		_instance.StartCoroutine(_instance.HighlightNearbyItems());
		_instance._audioSource.pitch = Random.Range(0.9f, 1.1f);
		_instance._audioSource.Play();
		return true;
	}

	private IEnumerator HighlightNearbyItems()
	{
		float                currentTime = 0f;
		float                endTime     = _compassPulse.main.startLifetime.constant;
		float                speed       = _compassPulse.main.startSpeed.constant;
		HashSet<CompassItem> pulsedItems = new HashSet<CompassItem>();
		while (currentTime < endTime)
		{
			currentTime += Time.deltaTime;
			float distance = speed * currentTime;
			_compassItems.ForEach(c =>
			{
				if (pulsedItems.Contains(c)) return;
				float containerDistance = Vector2.Distance(transform.position, c.transform.position);
				if (containerDistance > distance) return;
				c.Pulse();
				pulsedItems.Add(c);
			});
			yield return null;
		}
	}

	private IEnumerator ShowItems()
	{
		_showItemTimeCurrent = 0f;
		while (_showItemTimeCurrent < ShowItemTimeMax)
		{
			_showItemTimeCurrent += Time.deltaTime;
			if (_showItemTimeCurrent > ShowItemTimeMax) _showItemTimeCurrent = ShowItemTimeMax;
			float                            baseAlpha                       = 1 - _showItemTimeCurrent / ShowItemTimeMax;
			List<Tuple<float, float, float>> alphaRotations                  = GetItemsInRange(baseAlpha);
			SquashRotations(alphaRotations);
			yield return null;
		}
	}

	private void SquashRotations(List<Tuple<float, float, float>> alphaRotations)
	{
		alphaRotations.Sort((a, b) => a.Item2.CompareTo(b.Item2));
		for (int i = 0; i < 18; ++i)
		{
			float                            angleFrom        = i * 20;
			float                            angleTo          = angleFrom + 20;
			List<Tuple<float, float, float>> itemsWithinAngle = alphaRotations.Where(c => c.Item2 >= angleFrom && c.Item2 <= angleTo).ToList();
			if (itemsWithinAngle.Count == 0)
			{
				_indicators[i].SetAlpha(0f);
				continue;
			}

			float alpha           = 0;
			float angle           = 0;
			float nearestDistance = 1000;
			itemsWithinAngle.ForEach(tup =>
			{
				if (tup.Item3 >= nearestDistance) return;
				nearestDistance = tup.Item3;
				alpha           = tup.Item1;
				angle           = tup.Item2;
			});
			_indicators[i].SetAlpha(alpha);
			_indicators[i].SetRotation(angle);
		}
	}

	private List<Tuple<float, float, float>> GetItemsInRange(float baseAlpha)
	{
		_alphaRotations.Clear();
		_compassItems.ForEach(c =>
		{
			float distance = Vector2.Distance(transform.position, c.transform.position);
			if (distance > MaxDetectDistance) return;
			float inverseDistance = 1 - distance / MaxDetectDistance;
			float newAlpha        = baseAlpha * inverseDistance;
			float rotation        = AdvancedMaths.AngleFromUp(transform.position, c.transform.position);
			_alphaRotations.Add(Tuple.Create(newAlpha, rotation, distance));
		});
		return _alphaRotations;
	}

	public static void RegisterCompassItem(CompassItem compassItem)
	{
		_compassItems.Add(compassItem);
	}

	public static void UnregisterCompassItem(CompassItem compassItem)
	{
		_compassItems.Remove(compassItem);
	}
}