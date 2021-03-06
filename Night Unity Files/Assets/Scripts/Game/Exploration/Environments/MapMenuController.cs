﻿using System;
using System.Collections.Generic;
using DG.Tweening;
using Game.Characters;
using Game.Exploration.Regions;
using Game.Global;
using SamsHelper;
using SamsHelper.Libraries;
using UnityEngine;
using Facilitating.UIControllers;
using Game.Characters.CharacterActions;
using Game.Global.Tutorial;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Input;
using SamsHelper.ReactiveUI.Elements;
using SamsHelper.ReactiveUI.MenuSystem;
using Random = UnityEngine.Random;

namespace Game.Exploration.Environment
{
	public class MapMenuController : Menu, IInputListener
	{
		private List<Region> route;
		public  Transform    MapTransform;

		private readonly List<Tuple<Region, Region>>  _allRoutes     = new List<Tuple<Region, Region>>();
		private readonly Queue<Tuple<Region, Region>> _undrawnRoutes = new Queue<Tuple<Region, Region>>();
		private          float                        _nextRouteTime;
		private          float                        _currentTime;
		private          bool                         _isActive;
		private readonly List<RingDrawer>             _rings = new List<RingDrawer>();
		private static   Player                       _characterReturning;
		private          UIAttributeMarkerController  _gritMarker,   _willMarker;
		private          EnhancedText                 _teleportText, _willText;
		private          bool                         _seenTutorial;
		private          Tweener                      _teleportTween;
		private          bool                         _canTeleport;
		private static   MapMenuController            _instance;
		private          Region                       _nearestRegion;
		private          string                       _teleportControlText, _willControlText;
		private static   Player                       _player;

		public static Player CharacterReturning
		{
			get { return _characterReturning; }
			set { _characterReturning = value; }
		}

		protected override void Awake()
		{
			base.Awake();
			_gritMarker    = gameObject.FindChildWithName("Grit").FindChildWithName<UIAttributeMarkerController>("Bar");
			_willMarker    = gameObject.FindChildWithName("Will").FindChildWithName<UIAttributeMarkerController>("Bar");
			_teleportText  = gameObject.FindChildWithName<EnhancedText>("Teleport");
			_willText      = _willMarker.transform.parent.FindChildWithName<EnhancedText>("Text");
			_nextRouteTime = 2f / MapGenerator.Regions().Count;
			MapTransform   = GameObject.Find("Nodes").transform;
			_instance      = this;
			CreateMapRings();
			CreateRouteLinks();
		}

		public void Start()
		{
			MenuStateMachine.RegisterMenu(this);
			ControlTypeChangeListener controlTypeChangeListener = GetComponent<ControlTypeChangeListener>();
			controlTypeChangeListener.SetOnControllerInputChange(UpdateText);
		}

		private void UpdateText()
		{
			_teleportControlText = InputHandler.GetBindingForKey(InputAxis.Compass);
			_willControlText     = InputHandler.GetBindingForKey(InputAxis.Reload);
		}

		private void OnDestroy()
		{
			_instance = null;
		}

		private void ReturnFromCombat()
		{
			if (MenuStateMachine.CurrentMenu() == this) return;
			Open(CharacterReturning);
		}

		public override void Enter()
		{
			base.Enter();
			_isActive = true;
			_rings.ForEach(r => r.TweenColour(UiAppearanceController.InvisibleColour, Color.white, 0.5f));
			UpdateTeleportText();
			UpdateWill();
			ResourcesUiController.Hide();
			MapGenerator.Regions().ForEach(n => { n.ShowNode(_player); });
			MapMovementController.Instance().Enter(_player);
			AudioController.FadeInMusicMuffle();
			InputHandler.RegisterInputListener(this);
			ShowMapTutorial();
		}

		public override void Exit()
		{
			base.Exit();
			_rings.ForEach(r => r.TweenColour(Color.white, UiAppearanceController.InvisibleColour, 0.5f));
			_isActive = false;
			MapGenerator.Regions().ForEach(n => n.HideNode());
			MapMovementController.Instance().Exit();
			FadeAndDieTrailRenderer.ForceFadeAll();
			AudioController.FadeOutMusicMuffle();
			InputHandler.UnregisterInputListener(this);
			ResourcesUiController.Show();
			_player            = null;
			CharacterReturning = null;
		}

		public override void PreEnter()
		{
			UpdateGrit(0);
			UpdateWill();
		}

		private void UpdateTeleportText()
		{
			int stoneQuantity = Inventory.GetResourceQuantity("Mystic Shard");
			_canTeleport = stoneQuantity > 0;
			string teleportString            = "No Mystic Shards";
			if (_canTeleport) teleportString = "Teleport [" + _teleportControlText + "] (Consumes 1 Mystic Shard)";
			_teleportText.SetText(teleportString);
			_willText.SetText("Use Will [" + _willControlText + "]");
		}

		private void TryTeleport()
		{
			Region region = _nearestRegion;
			if (region == null) return;
			if (!_canTeleport) return;
			Inventory.DecrementResource("Mystic Shard", 1);
			if (region.GetRegionType() == RegionType.Gate) _player.TravelAction.ReturnToHomeInstant(true);
			else _player.TravelAction.TravelToInstant(region);
			CharacterReturning = null;
			Exit();
		}

		public void FadeTeleportText()
		{
			_teleportTween?.Kill();
			float target = _nearestRegion == null ? 0f : 1f;
			_teleportTween = _teleportText.GetText().DOFade(target, 1f);
		}

		private void ShowMapTutorial()
		{
			if (_seenTutorial || !TutorialManager.Active()) return;
			TutorialOverlay overlay = new TutorialOverlay(MapGenerator.GetInitialNode().MapNode().transform, 3, 3);
			TutorialManager.Instance.TryOpenTutorial(2, overlay);
			_seenTutorial = true;
		}

		private void CreateRouteLinks()
		{
			List<Region> _discovered = MapGenerator.SeenRegions();
			foreach (Region from in _discovered)
			{
				foreach (Region to in _discovered)
				{
					if (!from.Neighbors().Contains(to)) continue;
					_allRoutes.Add(Tuple.Create(from, to));
				}
			}
		}

		private void DrawBasicRoutes()
		{
			if (_undrawnRoutes.Count == 0)
			{
				_allRoutes.Shuffle();
				_allRoutes.ForEach(a => _undrawnRoutes.Enqueue(a));
			}

			_currentTime += Time.deltaTime;
			if (_currentTime < 1f / _allRoutes.Count) return;
			Tuple<Region, Region> link = _undrawnRoutes.Dequeue();
			Region                from = link.Item1;
			Region                to   = link.Item2;
			Vector3[]             rArr = new Vector3[Random.Range(2, 6)];
			for (int j = 0; j < rArr.Length; ++j)
			{
				if (j == 0)
				{
					rArr[j] = from.Position;
					continue;
				}

				if (j == rArr.Length - 1)
				{
					rArr[j] = to.Position;
					continue;
				}

				float   normalisedDistance = (float) j / rArr.Length;
				Vector2 pos                = AdvancedMaths.PointAlongLine(from.Position, to.Position, normalisedDistance);
				pos     = AdvancedMaths.RandomVectorWithinRange(pos, Random.Range(0.1f, 0.1f));
				rArr[j] = pos;
			}

			FadeAndDieTrailRenderer.CreatePale(rArr);
			_currentTime = 0f;
		}

		private void CreateMapRings()
		{
			GameObject ringPrefab = Resources.Load<GameObject>("Prefabs/Map/Map Ring");
			Transform  ringParent = GameObject.Find("Rings").transform;
			int        counter    = 0;
			float      alpha      = 0.2f;
			for (int i = 1; i <= 10; ++i)
			{
				int        ringRadius       = i * MapGenerator.MinRadius;
				GameObject ring             = Instantiate(ringPrefab, transform.position, ringPrefab.transform.rotation);
				float      rotateSpeed      = Mathf.Pow(0.9f, i - 1);
				if (i % 2 == 0) rotateSpeed = -rotateSpeed;
				ring.AddComponent<Rotate>().RotateSpeed = rotateSpeed;
				ring.layer                              = 23;
				ring.transform.SetParent(ringParent);
				ring.name = "Ring: distance " + i + " hours";
				RingDrawer ringDrawer = ring.GetComponent<RingDrawer>();
				ringDrawer.SetRadius(ringRadius);
				alpha *= 0.9f;
				switch (counter)
				{
					case 0:
						ringDrawer.UseBorder1();
						break;
					case 1:
						ringDrawer.UseBorder2();
						break;
					case 2:
						ringDrawer.UseBorder3();
						break;
				}

				ringDrawer.SetAlphaMultiplier(alpha);
				_rings.Add(ringDrawer);
				++counter;
				if (counter == 3) counter = 0;
			}

			_rings.ForEach(r => r.SetColor(UiAppearanceController.InvisibleColour));
		}

		private void DrawTargetRoute()
		{
			if (route.Count <= 1) return;
			_nextRouteTime -= Time.deltaTime;
			if (_nextRouteTime > 0) return;
			_nextRouteTime = Random.Range(0.2f, 0.5f);

			Vector3[] rArr = new Vector3[route.Count];
			for (int j = 0; j < route.Count; ++j)
			{
				Vector3 pos   = route[j].Position;
				float   range = j > 0 && j < route.Count - 1 ? Random.Range(0.25f, 0.5f) : 0.1f;
				pos     = AdvancedMaths.RandomVectorWithinRange(pos, range);
				rArr[j] = pos;
			}

			FadeAndDieTrailRenderer.CreateRed(rArr);
		}

		public void Update()
		{
			if (CharacterReturning != null) ReturnFromCombat();
			if (!_isActive) return;
			DrawBasicRoutes();
			if (route == null || route.Count == 1) return;
			DrawTargetRoute();
		}

		public void SetRoute(Region to)
		{
			route = RoutePlotter.RouteBetween(_player.TravelAction.GetCurrentRegion(), to);
		}

		private void TravelToRegion()
		{
			if (_nearestRegion == null) return;
			if (!CanAffordToTravel()) return;
			if (TutorialManager.Instance.IsTutorialVisible()) return;
			Travel travelAction = _player.TravelAction;
			travelAction.TravelTo(_nearestRegion, _nearestRegion.MapNode().GetDistance());
			CharacterReturning = null;
			MenuStateMachine.ShowMenu("Game Menu");
		}

		private bool CanAffordToTravel()
		{
			int  gritCost          = _nearestRegion.MapNode().GetGritCost();
			bool canAfford         = _player.CanAffordTravel(gritCost);
			bool travellingToGate  = _nearestRegion.GetRegionType() == RegionType.Gate;
			bool canAffordToTravel = canAfford || travellingToGate;
			return canAffordToTravel;
		}

		private void TryRestoreGrit()
		{
			CharacterAttribute grit = _player.Attributes.Get(AttributeType.Grit);
			CharacterAttribute will = _player.Attributes.Get(AttributeType.Will);
			if (will.CurrentValue() == 0 || grit.ReachedMax()) return;
			will.Decrement();
			grit.Increment();
			_gritMarker.SetValue(grit.Max, grit.CurrentValue(), 0);
			_willMarker.SetValue(will.Max, will.CurrentValue(), 0);
			MapGenerator.Regions().ForEach(n => { n.ShowNode(_player); });
		}

		public void UpdateGrit(int gritCost)
		{
			CharacterAttribute grit = _player.Attributes.Get(AttributeType.Grit);
			_gritMarker.SetValue(grit.Max, grit.CurrentValue(), -gritCost);
		}

		private void UpdateWill()
		{
			CharacterAttribute will = _player.Attributes.Get(AttributeType.Will);
			_willMarker.SetValue(will.Max, will.CurrentValue(), 0);
		}

		public void OnInputDown(InputAxis axis, bool isHeld, float direction = 0)
		{
			if (isHeld) return;
			switch (axis)
			{
				case InputAxis.Accept:
					TravelToRegion();
					break;
				case InputAxis.Fire:
					TravelToRegion();
					break;
				case InputAxis.Compass:
					TryTeleport();
					break;
				case InputAxis.Reload:
					TryRestoreGrit();
					break;
				case InputAxis.Cancel:
					if (CharacterReturning == null && !TutorialManager.Instance.IsTutorialVisible())
						MenuStateMachine.ShowMenu("Game Menu");
					break;
			}
		}

		public void OnInputUp(InputAxis axis)
		{
		}

		public void OnDoubleTap(InputAxis axis, float direction)
		{
		}

		public static MapMenuController Instance() => _instance;

		public void SetNearestRegion(Region nearestRegion)
		{
			_nearestRegion = nearestRegion;
		}

		public static void Open(Player playerCharacter)
		{
			if (_player != null) return;
			WorldState.Pause();
			_player = playerCharacter;
			MenuStateMachine.ShowMenu("Map Menu");
		}
	}
}