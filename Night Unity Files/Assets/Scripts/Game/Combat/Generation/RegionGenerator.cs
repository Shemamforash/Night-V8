using System;
using System.Collections.Generic;
using Game.Combat.Generation.Shrines;
using Game.Combat.Misc;
using Game.Exploration.Regions;
using Game.Global;
using NUnit.Framework;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Libraries;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Combat.Generation
{
    public abstract class RegionGenerator : MonoBehaviour
    {
        protected const float MinPolyWidth = 0.1f;
        protected const float SmallPolyWidth = 0.2f;
        protected const float MediumPolyWidth = 2f;
        protected const float LargePolyWidth = 3f;
        private Region _region;
        private List<Vector2> _availablePositions;
        private int _barrierNumber;
        protected readonly List<Barrier> barriers = new List<Barrier>();
        private const float CacheRadius = 5.5f, ShelterRadius = 5f;
        private float NoGoDistance = -1f;

        public void Initialise(Region region)
        {
            _region = region;
            Random.InitState(region.RegionID + WorldState.Seed);
            SetRegionWidth();
            WorldGrid.InitialiseGrid(_region.IsDynamic());
            GenerateFreshEnvironment();
            GenerateObjects();
            GenerateEdges();
            WorldGrid.FinaliseGrid();
        }

        private void RemoveBlockedArea()
        {
            if (_region.CharacterHere != null)
            {
                Polygon characterBlockingArea = WorldGrid.AddBlockingArea(Vector2.zero, ShelterRadius);
                WorldGrid.RemoveBarrier(characterBlockingArea);
            }

            if (_region.GetRegionType() == RegionType.Cache)
            {
                Polygon characterBlockingArea = WorldGrid.AddBlockingArea(Vector2.zero, CacheRadius);
                WorldGrid.RemoveBarrier(characterBlockingArea);
            }
        }

        private void SetRegionWidth()
        {
            if (_region.GetRegionType() == RegionType.Rite)
            {
                WorldGrid.SetCombatAreaWidth(15);
                return;
            }

            bool mustBeFullSize = _region.GetRegionType() == RegionType.Temple || _region.GetRegionType() == RegionType.Tomb;
            if (mustBeFullSize)
            {
                WorldGrid.SetCombatAreaWidth(30);
                return;
            }

            int difficulty = WorldState.Difficulty();
            int width = (int) (20 + difficulty / 5f);
            WorldGrid.SetCombatAreaWidth(width);
        }

        protected virtual void GenerateObjects()
        {
            GenerateShrine();
            _region.Fires.ForEach(f => f.CreateObject());
            _region.Containers.ForEach(c => c.CreateObject());
            _region.Barriers.ForEach(b => b.CreateObject());
            if (_region.GetRegionType() != RegionType.Shelter) return;
            RescueRingController.Generate();
        }

        private void GenerateEdges()
        {
            EdgeCollider2D edgeCollider = gameObject.AddComponent<EdgeCollider2D>();
            gameObject.layer = 13;
            float radius = WorldGrid.CombatAreaWidth / 2f - 0.1f;
            int numPoints = 500;
            Vector2[] edgePoints = new Vector2[numPoints];
            for (int i = 0; i < numPoints; ++i)
            {
                float angle = 2 * Mathf.PI * (i / (float) numPoints);
                float x = radius * Mathf.Cos(angle);
                float y = radius * Mathf.Sin(angle);
                edgePoints[i] = new Vector2(x, y);
            }

            edgeCollider.points = edgePoints;
        }

        private void GenerateFreshEnvironment()
        {
            if (_region.Generated()) return;
            NoGoDistance = GetCentreProtectedArea();
            _availablePositions = new List<Vector2>(AdvancedMaths.GetPoissonDiscDistribution(900, WorldGrid.CombatAreaWidth * 1.5f));
            GenerateBlockedArea();
            Generate();
            RemoveBlockedArea();
            _region.Barriers = barriers;
            WorldGrid.InitialiseGrid(_region.IsDynamic());
            _region.MarkGenerated();
        }

        private void CreateImpassablePoint(Vector2 position, float radius)
        {
            WorldGrid.AddBlockingArea(position, radius);
        }

        private void GenerateShrine()
        {
            switch (_region.GetRegionType())
            {
                case RegionType.Shrine:
                    RiteShrineBehaviour.Generate(_region);
                    break;
                case RegionType.Fountain:
                    FountainBehaviour.Generate(_region);
                    break;
                case RegionType.Monument:
                    SaveStoneBehaviour.Generate();
                    break;
                case RegionType.Cache:
                    CacheController.Generate();
                    break;
            }
        }

        protected float GetCentreProtectedArea()
        {
            switch (_region.GetRegionType())
            {
                case RegionType.Shelter:
                    if (_region.CharacterHere != null) return ShelterRadius;
                    return 0;
                case RegionType.Fountain:
                    return 3f;
                case RegionType.Shrine:
                    return 2f;
                case RegionType.Monument:
                    return 1.5f;
                case RegionType.Cache:
                    return CacheRadius;
                default:
                    return 0;
            }
        }

        private void GenerateBlockedArea()
        {
            if (NoGoDistance == 0) return;
            CreateImpassablePoint(Vector2.zero, NoGoDistance);
        }

        private void GenerateGenericRock(float radius, float radiusVariation, float smoothness, Vector2 position)
        {
            smoothness = Mathf.Clamp(1 - smoothness, 0.01f, 1f);
            radiusVariation = 1 - Mathf.Clamp(radiusVariation, 0f, 1f);

            Assert.IsTrue(radiusVariation <= 1);
            Assert.IsTrue(smoothness <= 1);

            float definition = smoothness * 150f;
            float angleIncrement;
            List<Vector2> barrierVertices = new List<Vector2>();
            int offset = Random.Range(-500, 500);
            for (float i = 0; i < 360; i += angleIncrement)
            {
                float angle = i * Mathf.Deg2Rad;
                Vector2 vertex = new Vector2();
                vertex.x = radius * Mathf.Cos(angle);
                vertex.y = radius * Mathf.Sin(angle);
                float noise = Mathf.PerlinNoise(vertex.x + offset, vertex.y + offset);
                vertex += Vector2.one * (radius / 4f) * noise * radiusVariation;
                barrierVertices.Add(vertex);
                angleIncrement = Random.Range(definition / 2f, definition);
            }

            new Barrier(barrierVertices, "Barrier " + GetObjectNumber(), position, barriers);
        }

        protected bool IsPositionInNoGoArea(float radius, Vector2 position)
        {
            return position.magnitude - radius < NoGoDistance;
        }

        protected Vector2? FindAndRemoveValidPosition(float radius, bool ignoreBounds = false)
        {
            for (int i = _availablePositions.Count - 1; i >= 0; --i)
            {
                Vector2 position = _availablePositions[i];
                if (!ignoreBounds && position.magnitude > WorldGrid.CombatMovementDistance / 2f - radius) continue;
                if (IsPositionInNoGoArea(radius, position)) continue;
                Cell c = WorldGrid.WorldToCellPosition(position, false);
                if (c == null) continue;
                if (!c.Reachable)
                {
                    _availablePositions.RemoveAt(i);
                    continue;
                }

                if (radius != 0)
                {
                    Vector2 topLeft = new Vector2(position.x - radius, position.y - radius);
                    Vector2 bottomRight = new Vector2(position.x + radius, position.y + radius);
                    if (!WorldGrid.IsSpaceAvailable(topLeft, bottomRight)) continue;
                }

                _availablePositions.RemoveAt(i);
                return c.Position;
            }

            return null;
        }

        private void GenerateFire()
        {
            Vector2? potentialPosition = FindAndRemoveValidPosition(0.4f);
            if (potentialPosition == null) return;
            Vector2 position = potentialPosition.Value;
            int numberOfStones = Random.Range(6, 10);
            float radius = 0.2f;
            int angle = 0;
            int angleStep = 360 / numberOfStones;
            while (numberOfStones > 0)
            {
                float randomisedAngle = (angle + Random.Range(-angleStep, angleStep)) * Mathf.Deg2Rad;
                float x = radius * Mathf.Cos(randomisedAngle) + position.x;
                float y = radius * Mathf.Sin(randomisedAngle) + position.y;
                GenerateGenericRock(0.05f, 0.2f, 0.5f, new Vector2(x, y));
                angle += angleStep;
                --numberOfStones;
            }

            CreateImpassablePoint(position, 0.4f);

            _region.Fires.Add(new EnemyCampfire(position));
        }

        public void GenerateOneRock(float minPolyWidth, float maxPolyWidth, float radiusVariation, float smoothness, Vector2 position)
        {
            float radius = Random.Range(minPolyWidth, maxPolyWidth);
            GenerateGenericRock(radius, radiusVariation, smoothness, position);
        }

        private void GenerateRocks(int number, float minPolyWidth, float maxPolyWidth, float radiusVariation, float smoothness)
        {
            while (number > 0)
            {
                float radius = Random.Range(minPolyWidth, maxPolyWidth);
                Vector2? potentialPosition = FindAndRemoveValidPosition(radius);
                if (potentialPosition == null) return;
                Vector2 position = potentialPosition.Value;
                GenerateGenericRock(radius, radiusVariation, smoothness, position);
                --number;
            }
        }

        protected void GenerateMediumRocks(int number = 1)
        {
            GenerateRocks(number, MediumPolyWidth, LargePolyWidth, 0.7f, 0.5f);
        }

        protected void GenerateSmallRocks(int number = 1)
        {
            GenerateRocks(number, SmallPolyWidth, MediumPolyWidth, 0.5f, 0.75f);
        }

        protected void GenerateTinyRocks(int number = 1)
        {
            GenerateRocks(number, MinPolyWidth, SmallPolyWidth, 0.1f, 0.75f);
        }

        protected int GetObjectNumber()
        {
            int num = _barrierNumber;
            ++_barrierNumber;
            return num;
        }

        protected virtual void PlaceItems()
        {
            _availablePositions = new List<Vector2>(AdvancedMaths.GetPoissonDiscDistribution(400, WorldGrid.CombatAreaWidth, false, 0.8f));
            _availablePositions.RemoveAll(p => p.magnitude < 2.5f);

            if (_region.GetRegionType() == RegionType.Danger)
            {
                int numFires = Random.Range(3, 6);
                while (numFires > 0)
                {
                    --numFires;
                    GenerateFire();
                }
            }

            JournalEntry journalEntry = JournalEntry.GetLoreEntry();
            if (!_region.ReadJournal && journalEntry != null)
                CreateContainer<JournalSource>()?.SetEntry(journalEntry);

            for (int i = 0; i < _region.WaterSourceCount; ++i)
                CreateContainer<WaterSource>();

            for (int i = 0; i < _region.FoodSourceCount; ++i)
                CreateContainer<FoodSource>();

            for (int i = 0; i < _region.ResourceSourceCount; ++i)
            {
                ResourceItem resource = ResourceTemplate.GetResource().Create();
                CreateContainer<Loot>()?.SetResource(resource);
            }
        }

        private T CreateContainer<T>() where T : ContainerController
        {
            Vector2? potentialPosition = FindAndRemoveValidPosition(0.4f);
            if (potentialPosition == null) return null;
            Vector2 position = potentialPosition.Value;
            CreateImpassablePoint(position, 0.5f);
            T t = (T) Activator.CreateInstance(typeof(T), position);
            _region.Containers.Add(t);
            return t;
        }

        protected abstract void Generate();
    }
}