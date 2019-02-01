using System;
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
        private static List<Region> route;
        public static Transform MapTransform;

        private readonly List<Tuple<Region, Region>> _allRoutes = new List<Tuple<Region, Region>>();
        private readonly Queue<Tuple<Region, Region>> _undrawnRoutes = new Queue<Tuple<Region, Region>>();
        private float _nextRouteTime;
        private float _currentTime;
        private bool _isActive;
        private readonly List<RingDrawer> _rings = new List<RingDrawer>();
        public static bool IsReturningFromCombat;
        private static UIAttributeMarkerController _gritMarker, _willMarker;
        private static EnhancedText _teleportText;
        private bool _seenTutorial;
        private static Tweener _teleportTween;
        private static bool _canTeleport;

        public override void Awake()
        {
            base.Awake();
            _gritMarker = gameObject.FindChildWithName("Grit").FindChildWithName<UIAttributeMarkerController>("Bar");
            _willMarker = gameObject.FindChildWithName("Will").FindChildWithName<UIAttributeMarkerController>("Bar");
            _teleportText = gameObject.FindChildWithName<EnhancedText>("Teleport");
            _nextRouteTime = 2f / MapGenerator.Regions().Count;
            MapTransform = GameObject.Find("Nodes").transform;
            CreateMapRings();
            CreateRouteLinks();
        }

        private void ReturnFromCombat()
        {
            if (MenuStateMachine.CurrentMenu() == this) return;
            MenuStateMachine.ShowMenu("Map Menu");
        }

        public override void Enter()
        {
            base.Enter();
            _isActive = true;
            _rings.ForEach(r => r.TweenColour(UiAppearanceController.InvisibleColour, Color.white, 0.5f));
            UpdateTeleportText();
            UpdateWill();
            ResourcesUiController.Hide();
            MapGenerator.Regions().ForEach(n => { n.ShowNode(); });
            MapMovementController.Enter();
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
            MapMovementController.Exit();
            FadeAndDieTrailRenderer.ForceFadeAll();
            AudioController.FadeOutMusicMuffle();
            InputHandler.UnregisterInputListener(this);
            ResourcesUiController.Show();
        }

        private void UpdateTeleportText()
        {
            int stoneQuantity = Inventory.GetResourceQuantity("Gate Stone");
            _canTeleport = stoneQuantity > 0;
            string teleportString = "No Gate Stones";
            if (_canTeleport) teleportString = "Teleport [T] (Consumes 1 Gate Stone)";
            _teleportText.SetText(teleportString);
        }

        private void TryTeleport()
        {
            Region region = MapMovementController.GetNearestRegion();
            if (region == null) return;
            if (!_canTeleport) return;
            Inventory.DecrementResource("Gate Stone", 1);
            if (region.GetRegionType() == RegionType.Gate) CharacterManager.SelectedCharacter.TravelAction.ReturnToHomeInstant();
            else CharacterManager.SelectedCharacter.TravelAction.TravelToInstant(region);
            IsReturningFromCombat = false;
        }

        public static void FadeTeleportText(Region region)
        {
            _teleportTween?.Kill();
            float target = region == null ? 0f : 1f;
            _teleportTween = _teleportText.GetText().DOFade(target, 1f);
        }

        private void ShowMapTutorial()
        {
            if (_seenTutorial || !TutorialManager.Active()) return;
            TutorialOverlay overlay = new TutorialOverlay(MapGenerator.GetInitialNode().MapNode().transform, 3, 3);
            TutorialManager.TryOpenTutorial(2, overlay);
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
            Region from = link.Item1;
            Region to = link.Item2;
            Vector3[] rArr = new Vector3[Random.Range(2, 6)];
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

                float normalisedDistance = (float) j / rArr.Length;
                Vector2 pos = AdvancedMaths.PointAlongLine(from.Position, to.Position, normalisedDistance);
                pos = AdvancedMaths.RandomVectorWithinRange(pos, Random.Range(0.1f, 0.1f));
                rArr[j] = pos;
            }

            FadeAndDieTrailRenderer.CreatePale(rArr);
            _currentTime = 0f;
        }

        private void CreateMapRings()
        {
            GameObject ringPrefab = Resources.Load<GameObject>("Prefabs/Map/Map Ring");
            Transform ringParent = GameObject.Find("Rings").transform;
            int counter = 0;
            float alpha = 0.2f;
            for (int i = 1; i <= 10; ++i)
            {
                int ringRadius = i * MapGenerator.MinRadius;
                GameObject ring = Instantiate(ringPrefab, transform.position, ringPrefab.transform.rotation);
                float rotateSpeed = Mathf.Pow(0.9f, i - 1);
                if (i % 2 == 0) rotateSpeed = -rotateSpeed;
                ring.AddComponent<Rotate>().RotateSpeed = rotateSpeed;
                ring.layer = 23;
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
                Vector3 pos = route[j].Position;
                float range = j > 0 && j < route.Count - 1 ? Random.Range(0.25f, 0.5f) : 0.1f;
                pos = AdvancedMaths.RandomVectorWithinRange(pos, range);
                rArr[j] = pos;
            }

            FadeAndDieTrailRenderer.CreateRed(rArr);
        }

        public void Update()
        {
            if (IsReturningFromCombat) ReturnFromCombat();
            if (!_isActive) return;
            DrawBasicRoutes();
            if (route == null || route.Count == 1) return;
            DrawTargetRoute();
        }

        public static void SetRoute(Region to)
        {
            route = RoutePlotter.RouteBetween(CharacterManager.SelectedCharacter.TravelAction.GetCurrentRegion(), to);
        }

        private void TravelToRegion()
        {
            Region region = MapMovementController.GetNearestRegion();
            if (region == null) return;
            if (!CanAffordToTravel(region)) return;
            if (TutorialManager.IsTutorialVisible()) return;
            Travel travelAction = CharacterManager.SelectedCharacter.TravelAction;
            travelAction.TravelTo(region, region.MapNode().GetDistance());
            IsReturningFromCombat = false;
            MenuStateMachine.ShowMenu("Game Menu");
        }

        private static bool CanAffordToTravel(Region region)
        {
            int gritCost = region.MapNode().GetGritCost();
            bool canAfford = CharacterManager.SelectedCharacter.CanAffordTravel(gritCost);
            bool travellingToGate = region.GetRegionType() == RegionType.Gate;
            bool canAffordToTravel = canAfford || travellingToGate;
            return canAffordToTravel;
        }

        private static void TryRestoreGrit()
        {
            CharacterAttribute grit = CharacterManager.SelectedCharacter.Attributes.Get(AttributeType.Grit);
            CharacterAttribute will = CharacterManager.SelectedCharacter.Attributes.Get(AttributeType.Will);
            if (will.CurrentValue() == 0 || grit.ReachedMax()) return;
            will.Decrement();
            grit.Increment();
            _gritMarker.SetValue(grit.Max, grit.CurrentValue(), 0);
            _willMarker.SetValue(will.Max, will.CurrentValue(), 0);
            MapGenerator.Regions().ForEach(n => { n.ShowNode(); });
        }

        public static void UpdateGrit(int gritCost)
        {
            CharacterAttribute grit = CharacterManager.SelectedCharacter.Attributes.Get(AttributeType.Grit);
            _gritMarker.SetValue(grit.Max, grit.CurrentValue(), -gritCost);
        }

        private void UpdateWill()
        {
            CharacterAttribute will = CharacterManager.SelectedCharacter.Attributes.Get(AttributeType.Will);
            _willMarker.SetValue(will.Max, will.CurrentValue(), 0);
        }

        public void OnInputDown(InputAxis axis, bool isHeld, float direction = 0)
        {
            if (isHeld) return;
            switch (axis)
            {
                case InputAxis.Fire:
                    TravelToRegion();
                    break;
                case InputAxis.Mouse:
                    TravelToRegion();
                    break;
                case InputAxis.TakeItem:
                    TryTeleport();
                    break;
                case InputAxis.Compass:
                    TryRestoreGrit();
                    break;
                case InputAxis.Menu:
                    if (!IsReturningFromCombat && !TutorialManager.IsTutorialVisible())
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
    }
}