using System.Collections.Generic;
using Game.Characters;
using Game.Combat.Generation.Shrines;
using Game.Combat.Misc;
using Game.Exploration.Regions;
using Game.Global;
using NUnit.Framework;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Libraries;
using UnityEngine;

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
            Random.InitState(region.RegionID);

            PathingGrid.InitialiseGrid();
            if (!_region.Visited())
                GenerateFreshEnvironment();

            _region.Visit();
            GenerateShrine();
            GenerateEchoes();
            _region.Fires.ForEach(f => f.CreateObject());
            _region.Containers.ForEach(c => c.CreateObject());
            _region.Barriers.ForEach(b => b.CreateObject());
            PathingGrid.FinaliseGrid();
        }

        private void GenerateEchoes()
        {
            List<Characters.Player> characters = CharacterManager.Characters;
            Helper.Shuffle(characters);
            foreach (Characters.Player c in characters)
            {
                if (!c.HasAvailableStoryLine()) continue;
                EchoBehaviour.Create(Helper.RandomInList(_region.EchoPositions), c);
                break;
            }
        }

        private void GenerateFreshEnvironment()
        {
            _availablePositions = new List<Vector2>(AdvancedMaths.GetPoissonDiscDistribution(1000, 1f, 3f, PathingGrid.CombatAreaWidth / 2f));
            for (int i = 0; i < 10; ++i)
            {
                Vector2? position = FindAndRemoveValidPosition();
                Assert.IsNotNull(position);
                _region.EchoPositions.Add(FindAndRemoveValidPosition().Value);
            }

            Generate();
            foreach (Barrier barrier in barriers)
            {
                if (!barrier.Valid) continue;
                _region.Barriers.Add(barrier);
            }

            _region.Barriers = barriers;
            PlaceShrine();
            PlaceItems();
            _region.Visit();
        }

        private void GenerateShrine()
        {
            BrandManager.Brand brand = CharacterManager.SelectedCharacter.BrandManager.NextBrand();
            if (brand == null) return;
            int randomShrine = Random.Range(0, 4);
            ShrineBehaviour.Generate(_region.ShrinePosition, (ShrineType) randomShrine, brand);
        }

        private void PlaceShrine()
        {
            for (int i = 0; i < _availablePositions.Count; ++i)
            {
                Vector2 topLeft = new Vector2(_availablePositions[i].x - 3f, _availablePositions[i].y - 3f);
                Vector2 bottomRight = new Vector2(_availablePositions[i].x + 3f, _availablePositions[i].y + 3f);
                if (_availablePositions[i].magnitude > 20) continue;
                if (PathingGrid.WorldToCellPosition(_availablePositions[i]) == null) continue;
                if (!PathingGrid.IsSpaceAvailable(topLeft, bottomRight)) continue;
                _region.ShrinePosition = _availablePositions[i];
                _availablePositions.RemoveAt(i);
                return;
            }
        }

        private void GenerateGenericRock(float radius, float radiusVariation, float smoothness, Vector2? position = null)
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

            AssignRockPosition(barrierVertices, position);
        }

        protected void PlaceFire()
        {
            for (int i = 0; i < _availablePositions.Count; ++i)
            {
                Vector2 topLeft = new Vector2(_availablePositions[i].x - 0.5f, _availablePositions[i].y - 0.5f);
                Vector2 bottomRight = new Vector2(_availablePositions[i].x + 0.5f, _availablePositions[i].y + 0.5f);
                if (_availablePositions[i].magnitude > 10) continue;
                if (PathingGrid.WorldToCellPosition(_availablePositions[i]) == null) continue;
                if (!PathingGrid.IsSpaceAvailable(topLeft, bottomRight)) continue;
                _region.Fires.Add(GenerateFire(_availablePositions[i]));
                _availablePositions.RemoveAt(i);
                return;
            }

            int randomIndex = Random.Range(0, barriers.Count);
            Barrier b = barriers[randomIndex];
            barriers.RemoveAt(randomIndex);
            _region.Fires.Add(GenerateFire(b.Position));
        }

        protected Vector2? FindAndRemoveValidPosition()
        {
            for (int i = _availablePositions.Count - 1; i >= 0; --i)
            {
                if (_availablePositions[i].magnitude > PathingGrid.CombatAreaWidth) continue;
                Cell c = PathingGrid.WorldToCellPosition(_availablePositions[i], false);
                if (c == null) continue;
                if (!c.Reachable) continue;
                _availablePositions.RemoveAt(i);
                return c.Position;
            }

            return null;
        }

        private EnemyCampfire GenerateFire(Vector2 position)
        {
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

            return new EnemyCampfire(position);
        }

        protected void AssignRockPosition(List<Vector2> barrierVertices, Vector2? position)
        {
            if (position == null) position = FindAndRemoveValidPosition();
            if (position == null) return;
            Barrier barrier = new Barrier(barrierVertices, "Barrier " + GetObjectNumber(), (Vector2) position);
            barriers.Add(barrier);
        }

        private void GenerateRocks(int number, float minPolyWidth, float maxPolyWidth, float radiusVariation, float smoothness)
        {
            while (number > 0)
            {
                GenerateGenericRock(Random.Range(minPolyWidth, maxPolyWidth), radiusVariation, smoothness);
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
            Helper.Shuffle(_availablePositions);
            for (int i = 0; i < _region.WaterSourceCount; ++i)
            {
                Vector2? position = FindAndRemoveValidPosition();
                Assert.IsNotNull(position);
                _region.Containers.Add(new WaterSource(position.Value));
            }

            for (int i = 0; i < _region.FoodSourceCount; ++i)
            {
                Vector2? position = FindAndRemoveValidPosition();
                Assert.IsNotNull(position);
                _region.Containers.Add(new FoodSource(position.Value));
            }

            for (int i = 0; i < _region.ResourceSourceCount; ++i)
            {
                Vector2? position = FindAndRemoveValidPosition();
                Assert.IsNotNull(position);
                InventoryItem resource = ResourceTemplate.GetResource().Create();
                Loot loot = new Loot(position.Value, "Resource");
                loot.AddToInventory(resource);
                _region.Containers.Add(loot);
                
            }
        }

        protected abstract void Generate();
    }
}