﻿using System;
using System.Collections.Generic;
using DG.Tweening;
using Game.Characters;
using Game.Exploration.Regions;
 using Game.Global;
 using SamsHelper;
using SamsHelper.Libraries;
using UnityEngine;
 using UnityEngine.Assertions;
 using Random = UnityEngine.Random;

namespace Game.Exploration.Environment
{
    public class MapGenerator : MonoBehaviour
    {
        private const int MapWidth = 120;
        private const int MinRadius = 6, MaxRadius = 9;
        private static List<Region> storedNodes = new List<Region>();
        private static Region initialNode;
        private static List<Region> route;
        public static Transform MapTransform;
        private float nextRouteTime = 0.3f;
        private static readonly List<GameObject> _routeTrails = new List<GameObject>();
        private readonly List<Tuple<Region, Region>> _allRoutes = new List<Tuple<Region, Region>>();
        private readonly Queue<Tuple<Region, Region>> _undrawnRoutes = new Queue<Tuple<Region, Region>>();
        private float currentTime;

        public void Awake()
        {
            _routeTrails.Clear();
            MapTransform = transform;
            storedNodes.ForEach(n => n.CreateObject());
            CreateMapRings();
            UpdateNodeColor();
            CreateRouteLinks();
        }

        private void CreateRouteLinks()
        {
            List<Region> _discovered = DiscoveredNodes();
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
                Helper.Shuffle(_allRoutes);
                _allRoutes.ForEach(a => _undrawnRoutes.Enqueue(a));
            }

            currentTime += Time.deltaTime;
            if (currentTime < 1f / _allRoutes.Count) return;
            Tuple<Region, Region> link = _undrawnRoutes.Dequeue();
            Region from = link.Item1;
            Region to = link.Item2;
            GameObject g = Instantiate(Resources.Load<GameObject>("Prefabs/Borders/Path Trail Faded"));
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

            g.transform.position = from.Position;
            Sequence s = DOTween.Sequence();
            s.Append(g.transform.DOPath(rArr, Random.Range(1f, 3f), PathType.CatmullRom, PathMode.TopDown2D));
            s.AppendCallback(() => g.GetComponent<FadeAndDieTrailRenderer>().StartFade(1f));
            currentTime = 0f;
        }

        private void CreateMapRings()
        {
            GameObject ringPrefab = Resources.Load<GameObject>("Prefabs/Map/Map Ring");
            for (int i = 1; i <= 10; ++i)
            {
                int ringRadius = i * MinRadius;
                GameObject ring = Instantiate(ringPrefab, transform.position, ringPrefab.transform.rotation);
                ring.transform.SetParent(transform);
                ring.name = "Ring: distance " + i + " hours";
                RingDrawer ringDrawer = ring.GetComponent<RingDrawer>();
                ringDrawer.DrawCircle(ringRadius);
                float alpha = 1f / 9f * i + 1f / 9f;
                alpha = 1 - alpha;
                ringDrawer.SetColor(new Color(1, 1, 1, alpha));
            }
        }

        public static Region GetInitialNode()
        {
            return initialNode;
        }

        public static void Generate()
        {
            storedNodes = RegionManager.GenerateRegions();
            initialNode = storedNodes[0];

            bool succeeded = false;
            while (!succeeded)
            {
                storedNodes.ForEach(s =>
                {
                    s.Reset();
                });
                List<Vector2> samples = AdvancedMaths.GetPoissonDiscDistribution(storedNodes.Count, MinRadius, MaxRadius, MapWidth / 2f, true);
                for (int i = 0; i < samples.Count; ++i)
                {
                    Vector2Int point = new Vector2Int((int) samples[i].x, (int) samples[i].y);
                    storedNodes[i].SetPosition(point);
                }
                succeeded = ConnectNodes();
            }
            initialNode.Discover();
            storedNodes.ForEach(r => r.Discover());
        }

        private static Graph CreateMinimumSpanningTree()
        {
            Graph map = new Graph();
            storedNodes.ForEach(n => map.AddNode(n));
            map.SetRootNode(initialNode);
            map.ComputeMinimumSpanningTree();
            map.Edges().ForEach(edge => { edge.A.AddNeighbor(edge.B); });
            return map;
        }

        private static bool ConnectNodes()
        {
            Graph map = CreateMinimumSpanningTree();
            try
            {
                SetMaxNodeDepth(8 + WorldState.Difficulty() * 3, map);
            }
            catch (Exception)
            {
                return false;
            }
            AddRandomLinks();
            return true;
        }

        private static void SetMaxNodeDepth(int maxDepth, Graph map)
        {
            List<Tuple<Region, Region>> edges = new List<Tuple<Region, Region>>();
            for (int i = 0; i < storedNodes.Count; ++i)
            {
                for (int j = i + 1; j < storedNodes.Count; ++j)
                {
                    Region from = storedNodes[i];
                    Region to = storedNodes[j];
                    if (from.Neighbors().Contains(to)) continue;
                    float distance = Vector2.Distance(from.Position, to.Position);
                    if (distance > MaxRadius) continue;
                    edges.Add(Tuple.Create(from, to));
                }
            }

            map.CalculateNodeDepths();
            float currentMaxNodeDepth = map.MaxDepth();
            while (currentMaxNodeDepth > maxDepth && edges.Count > 0)
            {
                edges.Sort((a, b) =>
                {
                    float deltaDepthA = Mathf.Abs(a.Item1.Depth - a.Item2.Depth);
                    float deltaDepthB = Mathf.Abs(b.Item1.Depth - b.Item2.Depth);
                    return -deltaDepthA.CompareTo(deltaDepthB);
                });
                Tuple<Region, Region> edge = edges[0];
                edges.RemoveAt(0);
                edge.Item1.AddNeighbor(edge.Item2);
                map.CalculateNodeDepths();
                currentMaxNodeDepth = map.MaxDepth();
            }

            if (currentMaxNodeDepth > maxDepth) throw new Exception();
        }
        
        private static void AddRandomLinks()
        {
            List<Region> Regions = new List<Region>(storedNodes);
            foreach (Region current in Regions)
            {
                storedNodes.Sort((a, b) => Vector2.Distance(current.Position, a.Position).CompareTo(Vector2.Distance(current.Position, b.Position)));
                storedNodes.ForEach(n =>
                {
                    if (n == current) return;
                    if (current.Neighbors().Count >= 2) return;
                    if (Vector2.Distance(current.Position, n.Position) > MaxRadius) return;
                    current.AddNeighbor(n);
                });
            }
        }

        public static void SetRoute(Region to)
        {
            _routeTrails.ForEach(g =>
            {
                if (g == null) return;
                FadeAndDieTrailRenderer fad = g.GetComponent<FadeAndDieTrailRenderer>();
                fad.StartFade(1);
            });
            _routeTrails.Clear();
            route = RoutePlotter.RouteBetween(CharacterManager.SelectedCharacter.TravelAction.GetCurrentNode(), to);
//            Camera.main.GetComponent<FitScreenToRoute>().FitRoute(route);
        }

        private void DrawTargetRoute()
        {
            if (route.Count <= 1) return;
            nextRouteTime -= Time.deltaTime;
            if (nextRouteTime > 0) return;
            nextRouteTime = Random.Range(0.2f, 0.5f);
            GameObject g = Instantiate(Resources.Load<GameObject>("Prefabs/Borders/Path Trail"));
            _routeTrails.Add(g);
            g.transform.position = route[0].Position;
            Vector3[] rArr = new Vector3[route.Count];
            for (int j = 0; j < route.Count; ++j)
            {
                Vector3 pos = route[j].Position;
                if (j > 0 && j < route.Count - 1)
                {
                    pos = AdvancedMaths.RandomVectorWithinRange(pos, Random.Range(0.25f, 0.5f));
                }
                else
                {
                    pos = AdvancedMaths.RandomVectorWithinRange(pos, 0.1f);
                }

                rArr[j] = pos;
            }

            Sequence sequence = DOTween.Sequence();
            sequence.Append(g.transform.DOPath(rArr, Random.Range(1f, 3f), PathType.CatmullRom, PathMode.TopDown2D));
            sequence.AppendCallback(() => _routeTrails.Remove(g));
        }

        public void Update()
        {
            DrawBasicRoutes();
            if (route == null || route.Count == 1) return;
            DrawTargetRoute();
        }

        public static List<Region> DiscoveredNodes()
        {
            return storedNodes.FindAll(n => n.Seen());
        }

        public static void UpdateNodeColor()
        {
//            foreach (Region n in storedNodes) n.UpdateColor();
        }
    }
}