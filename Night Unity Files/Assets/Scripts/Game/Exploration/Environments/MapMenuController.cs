using System;
using System.Collections.Generic;
using Game.Characters;
using Game.Exploration.Regions;
using Game.Global;
using SamsHelper;
using SamsHelper.Libraries;
using UnityEngine;
using Facilitating.UIControllers;
using Game.Global.Tutorial;
using SamsHelper.ReactiveUI.Elements;
using SamsHelper.ReactiveUI.MenuSystem;
using Random = UnityEngine.Random;

namespace Game.Exploration.Environment
{
    public class MapMenuController : Menu
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
        private static CloseButtonController _closeButton;
        private static UIAttributeMarkerController _gritMarker;

        public override void Awake()
        {
            base.Awake();
            _closeButton = gameObject.FindChildWithName<CloseButtonController>("Close Button");
            _closeButton.SetOnClick(MapMovementController.ReturnToGame);
            _gritMarker = gameObject.FindChildWithName("Grit").FindChildWithName<UIAttributeMarkerController>("Bar");
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
            MapGenerator.Regions().ForEach(n => { n.ShowNode(); });
            MapMovementController.Enter(CharacterManager.SelectedCharacter);
            AudioController.FadeInMuffle();
            List<TutorialOverlay> overlays = new List<TutorialOverlay>
            {
                new TutorialOverlay(MapGenerator.GetInitialNode().MapNode().transform, 400, 400, GameObject.Find("Canvas").GetComponent<Canvas>(), Camera.main)
            };
            TutorialManager.TryOpenTutorial(2, overlays);
        }

        public override void Exit()
        {
            base.Exit();
            _rings.ForEach(r => r.TweenColour(Color.white, UiAppearanceController.InvisibleColour, 0.5f));
            _isActive = false;
            MapGenerator.Regions().ForEach(n => n.HideNode());
            MapMovementController.Exit();
            FadeAndDieTrailRenderer.ForceFadeAll();
            AudioController.FadeOutMuffle();
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

        public static void FlashCloseButton()
        {
            _closeButton.Flash();
        }

        public static UIAttributeMarkerController GritMarker()
        {
            return _gritMarker;
        }
    }
}