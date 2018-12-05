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
        private const float MinPolyWidth = 0.1f, SmallPolyWidth = 0.2f, MediumPolyWidth = 2f, LargePolyWidth = 3f;
        private Region _region;
        private List<Vector2> _availablePositions;
        private int _barrierNumber;
        protected readonly List<Barrier> barriers = new List<Barrier>();

        public void Initialise(Region region)
        {
            _region = region;
            Random.InitState(region.RegionID + WorldState.Seed);
            SetRegionWidth();
            PathingGrid.InitialiseGrid(_region.GetRegionType() != RegionType.Rite);
            GenerateFreshEnvironment();
            GenerateObjects();
            GenerateEdges();
            _region.Visit();
            GenerateCharacter();
            PathingGrid.FinaliseGrid();
        }

        private void SetRegionWidth()
        {
            if (_region.GetRegionType() == RegionType.Rite)
            {
                PathingGrid.SetCombatAreaWidth(20);
                return;
            }

            bool mustBeFullSize = _region.GetRegionType() == RegionType.Temple || _region.GetRegionType() == RegionType.Tomb || _region.GetRegionType() == RegionType.Nightmare;
            if (mustBeFullSize)
            {
                PathingGrid.SetCombatAreaWidth(30);
                return;
            }

            int difficulty = WorldState.Difficulty();
            int width = (int) (20 + difficulty / 5f);
            PathingGrid.SetCombatAreaWidth(width);
        }

        protected virtual void GenerateObjects()
        {
            GenerateShrine();
            _region.Fires.ForEach(f => f.CreateObject());
            _region.Containers.ForEach(c => c.CreateObject());
            _region.Barriers.ForEach(b => b.CreateObject());
        }

        private void GenerateEdges()
        {
            EdgeCollider2D edgeCollider = gameObject.AddComponent<EdgeCollider2D>();
            gameObject.layer = 13;
            float radius = PathingGrid.CombatAreaWidth / 2f - 0.1f;
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

        private void GenerateCharacter()
        {
            if (_region.CharacterHere == null) return;
            ShelterCharacterBehaviour.Generate(_region.CharacterPosition);
        }

        private void GenerateFreshEnvironment()
        {
            if (_region.Generated()) return;
            _availablePositions = new List<Vector2>(AdvancedMaths.GetPoissonDiscDistribution(900, PathingGrid.CombatAreaWidth * 1.5f));
            Generate();
            _region.Barriers = barriers;
            PathingGrid.InitialiseGrid(_region.GetRegionType() != RegionType.Rite);
            _region.MarkGenerated();
        }

        private void CreateImpassablePoint(Vector2 position, float radius)
        {
            PathingGrid.AddBlockingArea(position, radius);
        }

        private void GenerateShrine()
        {
            if (_region.GetRegionType() == RegionType.Shrine)
            {
                RiteShrineBehaviour.Generate(_region);
            }

            if (_region.GetRegionType() == RegionType.Fountain)
            {
                FountainBehaviour.Generate(_region);
            }

            if (_region.GetRegionType() == RegionType.Monument)
            {
                SaveStoneBehaviour.Generate();
            }
        }

        protected bool ShouldPlaceShrine()
        {
            RegionType regionType = _region.GetRegionType();
            bool validRegion = regionType == RegionType.Monument || regionType == RegionType.Fountain || regionType == RegionType.Shrine;
            return validRegion;
        }

        protected void PlaceShrine()
        {
            if (!ShouldPlaceShrine()) return;
            CreateImpassablePoint(Vector2.zero, 1.5f);
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

        protected Vector2? FindAndRemoveValidPosition(float radius, bool ignoreBounds = false)
        {
            for (int i = _availablePositions.Count - 1; i >= 0; --i)
            {
                Vector2 position = _availablePositions[i];
                if (!ignoreBounds && position.magnitude > PathingGrid.CombatMovementDistance / 2f - radius) continue;
                Cell c = PathingGrid.WorldToCellPosition(position, false);
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
                    if (!PathingGrid.IsSpaceAvailable(topLeft, bottomRight)) continue;
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
            _availablePositions = new List<Vector2>(AdvancedMaths.GetPoissonDiscDistribution(400, PathingGrid.CombatAreaWidth, false, 0.8f));
            if (_region.CharacterHere != null)
            {
                _availablePositions.Sort((a, b) => -a.magnitude.CompareTo(b.magnitude));
                for (int i = _availablePositions.Count - 1; i >= 0; --i)
                {
                    if (!PathingGrid.WorldToCellPosition(_availablePositions[i]).Reachable) continue;
                    _region.CharacterPosition = _availablePositions[i];
                    _availablePositions.RemoveAt(i);
                    break;
                }

                _availablePositions.Shuffle();
            }

            if (_region.GetRegionType() == RegionType.Danger)
            {
                int numFires = Random.Range(3, 6);
                while (numFires > 0)
                {
                    --numFires;
                    GenerateFire();
                }
            }

            JournalEntry journalEntry = JournalEntry.GetEntry();
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