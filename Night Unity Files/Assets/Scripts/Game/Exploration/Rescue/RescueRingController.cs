using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Extensions;
using Game.Characters;
using Game.Combat.Generation;
using Game.Combat.Misc;
using Game.Gear;
using SamsHelper.Libraries;
using UnityEngine;
using Random = UnityEngine.Random;

public class RescueRingController : MonoBehaviour
{
	private static   GameObject                         _prefab;
	private static   RescueRingController               _instance;
	private readonly List<RescuePuzzleButtonController> _buttons       = new List<RescuePuzzleButtonController>();
	private readonly Dictionary<SpriteRenderer, int>    _ringRotations = new Dictionary<SpriteRenderer, int>();
	public           bool                               _active        = true;
	private          SpriteRenderer                     _ringInner,   _ringMiddle, _ringOuter, _lockPoints;
	private          AudioSource                        _rotateAudio, _finishAudio;
	private          UiSkillUpgradeEffectController     _successEffect;
	private          Sequence                           _waitTween;

	private void Awake()
	{
		_instance      = this;
		_rotateAudio   = GetComponent<AudioSource>();
		_finishAudio   = gameObject.FindChildWithName<AudioSource>("Upgrade");
		_successEffect = gameObject.FindChildWithName<UiSkillUpgradeEffectController>("Upgrade");
		_ringInner     = gameObject.FindChildWithName<SpriteRenderer>("Inner");
		_ringMiddle    = gameObject.FindChildWithName<SpriteRenderer>("Middle");
		_ringOuter     = gameObject.FindChildWithName<SpriteRenderer>("Outer");
		_lockPoints    = gameObject.FindChildWithName<SpriteRenderer>("Lock");
		_lockPoints.SetAlpha(0.25f);
		_ringRotations.Add(_ringInner,  0);
		_ringRotations.Add(_ringMiddle, 0);
		_ringRotations.Add(_ringOuter,  0);
		SetRingAlpha(0.75f);
		AddButton("Anticlockwise A", () => RotateGroupA(1));
		AddButton("Anticlockwise B", () => RotateGroupB(1));
		AddButton("Anticlockwise C", () => RotateGroupC(1));
		AddButton("Clockwise A",     () => RotateGroupA(-1));
		AddButton("Clockwise B",     () => RotateGroupB(-1));
		AddButton("Clockwise C",     () => RotateGroupC(-1));
		if (CharacterManager.CurrentRegion().RingChallengeComplete)
			SetComplete(0f);
		else
			RandomiseLock();
	}

	public static void Generate()
	{
		if (_prefab == null) _prefab = Resources.Load<GameObject>("Prefabs/Combat/Buildings/Rescue Rings");
		Instantiate(_prefab, Vector3.zero, Quaternion.identity);
	}

	private void RandomiseLock()
	{
		if (CharacterManager.CurrentRegion().CharacterHere == null)
		{
			Loot loot = new Loot(Vector2.zero);
			loot.SetItem(Inscription.Generate(true));
		}
		else
		{
			ShelterCharacterBehaviour.Generate();
		}

		SetInitialLockPosition(_ringInner,  Random.Range(1, 5));
		SetInitialLockPosition(_ringMiddle, Random.Range(1, 5));
		SetInitialLockPosition(_ringOuter,  Random.Range(1, 5));
	}

	private void SetInitialLockPosition(SpriteRenderer ring, int rotation)
	{
		_ringRotations[ring]    = rotation;
		ring.transform.rotation = Quaternion.Euler(0, 0, rotation * 72);
	}

	private void SetRingAlpha(float alpha)
	{
		_ringInner.SetAlpha(alpha);
		_ringMiddle.SetAlpha(alpha);
		_ringOuter.SetAlpha(alpha);
	}

	private void RotateVal(SpriteRenderer ring, int dir)
	{
		int current = _ringRotations[ring];
		current += dir;
		if (current < 0)
		{
			current = 4;
		}
		else if (current > 4) current = 0;

		_ringRotations[ring] = current;
		Vector3 finalRotation = new Vector3();
		finalRotation.z = current * 72;
		ring.SetAlpha(1f);
		ring.DOFade(0.75f, 1.5f);
		ring.transform.DORotate(finalRotation, 1.5f);
	}

	private void StartWaitTween()
	{
		_waitTween = DOTween.Sequence();
		_buttons.ForEach(b => b.DisableButton());
		_rotateAudio.Play();
		_waitTween.AppendInterval(1.5f);
		_waitTween.AppendCallback(() =>
		{
			_buttons.ForEach(b => b.EnableButton());
			_rotateAudio.Stop();
		});
		_waitTween.AppendCallback(CheckAligned);
	}

	private void RotateGroupA(int dir)
	{
		RotateVal(_ringInner, dir);
		RotateVal(_ringOuter, dir);
		StartWaitTween();
	}

	private void RotateGroupB(int dir)
	{
		RotateVal(_ringInner,  dir);
		RotateVal(_ringMiddle, dir);
		StartWaitTween();
	}

	private void RotateGroupC(int dir)
	{
		RotateVal(_ringOuter,  dir);
		RotateVal(_ringMiddle, dir);
		StartWaitTween();
	}

	public static bool Active() => Instance() != null && Instance()._active;

	private void CheckAligned()
	{
		if (_ringRotations.Values.Any(v => v != 0)) return;
		_successEffect.Activate();
		ShelterCharacterBehaviour.Instance().Free();
		_lockPoints.SetAlpha(1f);
		_lockPoints.DOFade(0f, 2f);
		SetComplete(1.5f);
		_finishAudio.Play();
	}

	private void SetComplete(float duration)
	{
		_buttons.ForEach(b => b.DisableButton());
		_ringRotations.Keys.ToList().ForEach(g => { g.GetComponent<SpriteRenderer>().DOFade(0.25f, duration); });
		Destroy(gameObject.FindChildWithName("Centre"));
		Destroy(this);
		_active = false;
		CharacterManager.CurrentRegion().RingChallengeComplete = true;
	}

	private void AddButton(string buttonName, Action onPress)
	{
		RescuePuzzleButtonController button = gameObject.FindChildWithName<RescuePuzzleButtonController>(buttonName);
		button.SetOnPress(onPress);
		_buttons.Add(button);
	}

	private void OnDestroy()
	{
		_instance = null;
	}

	public static RescueRingController Instance() => _instance;
}