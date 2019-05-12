using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Extensions;
using Facilitating.UIControllers;
using Game.Characters;
using Game.Combat.Misc;
using Game.Combat.Player;
using Game.Exploration.Regions;
using Game.Global.Tutorial;
using NUnit.Framework;

using SamsHelper.Input;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Combat.Generation.Shrines
{
	public class RiteShrineBehaviour : BasicShrineBehaviour, ICombatEvent
	{
		private static GameObject                  _riteShrinePrefab;
		private static RiteShrineBehaviour         _instance;
		private        List<Brand>                 _brandChoice;
		private        RiteColliderBehaviour       _collider1, _collider2, _collider3;
		private        List<RiteColliderBehaviour> _colliders;
		private        string                      _controlText;
		private        Region                      _region;
		private        bool                        _seenTutorial;
		private        int                         _targetBrand = -1;

		public float InRange()
		{
			ShowRiteTutorial();
			return _targetBrand;
		}

		public string GetEventText()
		{
			Brand brand = _brandChoice[_targetBrand];
			return "Accept the " + brand.GetDisplayName() + " [" + _controlText + "]\n<size=30>" + brand.GetRequirementText() + "</size>";
		}

		public void Activate()
		{
			BrandManager brandManager = PlayerCombat.Instance.Player.BrandManager;
			Brand        brand        = _brandChoice[_targetBrand];
			Assert.IsNotNull(brand);
			if (brandManager.TryActivateBrand(brand))
			{
				ActivateBrand();
				return;
			}

			BrandReplaceMenuController.Show(this, brand);
		}

		public void Awake()
		{
			_instance  = this;
			_collider1 = gameObject.FindChildWithName<RiteColliderBehaviour>("Collider 1");
			_collider2 = gameObject.FindChildWithName<RiteColliderBehaviour>("Collider 2");
			_collider3 = gameObject.FindChildWithName<RiteColliderBehaviour>("Collider 3");
			_colliders = new List<RiteColliderBehaviour>(new[] {_collider1, _collider2, _collider3});

			PolygonCollider2D col    = gameObject.FindChildWithName<PolygonCollider2D>("Triangle");
			List<Vector2>     points = col.points.ToList();
			Polygon           b      = new Polygon(points, Vector2.zero);
			WorldGrid.AddBarrier(b);
		}

		public void Start()
		{
			ControlTypeChangeListener controlTypeChangeListener = GetComponent<ControlTypeChangeListener>();
			controlTypeChangeListener.SetOnControllerInputChange(UpdateText);
		}

		private void UpdateText()
		{
			_controlText = InputHandler.GetBindingForKey(InputAxis.TakeItem);
		}

		public static RiteShrineBehaviour Instance() => _instance;

		private void OnDestroy()
		{
			_instance = null;
		}

		public static void Generate(Region region)
		{
			if (_riteShrinePrefab == null) _riteShrinePrefab = Resources.Load<GameObject>("Prefabs/Combat/Buildings/Rite Shrine");
			GameObject riteShrineObject                      = Instantiate(_riteShrinePrefab);
			riteShrineObject.GetComponent<RiteShrineBehaviour>().SetRites(region);
			riteShrineObject.transform.position = Vector2.zero;
			WorldGrid.AddBlockingArea(Vector2.zero, 1.5f);
		}

		private void SetRites(Region region)
		{
			_region = region;
			bool ritesRemain = region.RitesRemain;
			_brandChoice = CharacterManager.SelectedCharacter.BrandManager.GetBrandChoice();
			if (!ritesRemain) _brandChoice.Clear();
			if (_brandChoice.Count < 3) StopCandles(_collider3);
			if (_brandChoice.Count < 2) StopCandles(_collider2);
			if (_brandChoice.Count != 0) return;
			StopCandles(_collider1);
			Destroy(this);
		}

		public void EnterShrineCollider(RiteColliderBehaviour riteColliderBehaviour)
		{
			if (riteColliderBehaviour == _collider1)
			{
				_targetBrand = 0;
			}
			else if (riteColliderBehaviour == _collider2)
			{
				_targetBrand = 1;
			}
			else if (riteColliderBehaviour == _collider3)
			{
				_targetBrand = 2;
			}
		}

		public void ExitShrineCollider()
		{
			_targetBrand = -1;
		}

		private void StopCandles(RiteColliderBehaviour riteColliderBehaviour)
		{
			riteColliderBehaviour.GetComponent<ParticleSystem>().Stop();
			foreach (ParticleSystem candle in riteColliderBehaviour.transform.Find("Candles").GetComponentsInChildren<ParticleSystem>())
			{
				candle.Stop();
				candle.Clear();
			}

			CompassItem compass = GetComponent<CompassItem>();
			if (compass != null) compass.Die();
			_colliders.Remove(riteColliderBehaviour);
			Destroy(riteColliderBehaviour);
		}

		private void FadeCandles()
		{
			_colliders.ForEach(c =>
			{
				Transform riteTransform = c.transform;
				riteTransform.GetComponent<ParticleSystem>().Stop();
				foreach (ParticleSystem candle in riteTransform.Find("Candles").GetComponentsInChildren<ParticleSystem>())
					StartCoroutine(FadeCandle(candle));
				Destroy(c);
			});
			_colliders.Clear();
			CompassItem compass = GetComponent<CompassItem>();
			if (compass != null) compass.Die();
		}

		private IEnumerator FadeCandle(ParticleSystem candle)
		{
			ParticleSystem.EmissionModule emission      = candle.emission;
			float                         startEmission = emission.rateOverTime.constant;
			float                         currentTime   = 1f;
			while (currentTime > 0f)
			{
				if (!CombatManager.Instance().IsCombatActive()) yield return null;
				currentTime -= Time.deltaTime;
				float newEmission = startEmission * currentTime / 2f;
				emission.rateOverTime = newEmission;
				yield return null;
			}

			emission.rateOverTime = 0f;
		}

		private void ShowRiteTutorial()
		{
			if (_seenTutorial || !TutorialManager.Active()) return;
			if (_targetBrand == -1) return;
			TutorialManager.Instance.TryOpenTutorial(16, new TutorialOverlay());
			_seenTutorial = true;
		}

		public void ActivateBrand()
		{
			if (Triggered) return;
			Triggered = true;
			FadeCandles();
			_region.RitesRemain = false;
			CombatLogController.PostLog("Accepted the " + _brandChoice[_targetBrand].GetDisplayName());
			_brandChoice[_targetBrand] = null;
			_targetBrand               = -1;
			GetComponent<AudioSource>().Play();
			if (_brandChoice.TrueForAll(b => b == null)) GetComponent<CompassItem>().Die();
		}

		protected override void StartShrine()
		{
		}
	}
}