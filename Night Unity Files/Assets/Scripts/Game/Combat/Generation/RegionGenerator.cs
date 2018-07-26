using System.Collections.Generic;
using Game.Characters;
using Game.Combat.Generation.Shrines;
using Game.Combat.Misc;
using Game.Exploration.Environment;
using Game.Exploration.Regions;
using Game.Global;
using NUnit.Framework;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Combat.Generation
{
    public abstract class RegionGenerator : MonoBehaviour
    {
        private const float MinPolyWidth = 0.1f, SmallPolyWidth = 0.2f, MediumPolyWidth = 2f, LargePolyWidth = 3f;
        protected Region _region;
        private List<Vector2> _availablePositions;
        private int _barrierNumber;
        protected readonly List<Barrier> barriers = new List<Barrier>();

        public void Initialise(Region region)
        {
            _region = region;
            Random.InitState(region.RegionID + WorldState.Seed);
            PathingGrid.InitialiseGrid();
            GenerateFreshEnvironment();
            GenerateObjects();
            _region.Visit();
            PathingGrid.FinaliseGrid();
        }
        
        protected virtual void GenerateObjects()
        {
            GenerateShrine();
            GenerateEchoes();
            if (_region.HealShrinePosition != null)
            {
                HealShrineBehaviour.CreateObject(_region.HealShrinePosition.Value);
            }
            _region.Fires.ForEach(f => f.CreateObject());
            _region.Containers.ForEach(c => c.CreateObject());
            _region.Barriers.ForEach(b => b.CreateObject());
            GenerateCharacter();
        }

        private void GenerateCharacter()
        {
            if (_region._characterHere == null) return;
            ShelterCharacterBehaviour.Generate(_region.CharacterPosition);
        }

        private void GenerateFreshEnvironment()
        {
            if (_region.Visited()) return;
            _availablePositions = new List<Vector2>(AdvancedMaths.GetPoissonDiscDistribution(1000, 1f, 3f, PathingGrid.CombatAreaWidth / 2f));
            Generate();
            _region.Barriers = barriers;
            PathingGrid.InitialiseGrid();
        }

        protected void PlaceEchoes()
        {
            for (int i = 0; i < 10; ++i)
            {
                Vector2 position = FindAndRemoveValidPosition();
                _region.EchoPositions.Add(FindAndRemoveValidPosition());
                CreateImpassablePoint(position);
            }
        }

        protected void RemoveInvalidPoints()
        {
            _availablePositions.RemoveAll(p =>
            {
                Cell c = PathingGrid.WorldToCellPosition(p, false);
                if (c == null) return false;
                return !PathingGrid.WorldToCellPosition(p).Reachable;
            });
        }

        private void GenerateEchoes()
        {
            List<Characters.Player> characters = CharacterManager.Characters;
            Helper.Shuffle(characters);
            foreach (Characters.Player c in characters)
            {
                if (!c.HasAvailableStoryLine()) continue;
                Vector2 echoPosition = Helper.RandomInList(_region.EchoPositions);
                EchoBehaviour.Create(echoPosition, c);
                break;
            }
        }

        private void CreateImpassablePoint(Vector2 position)
        {
            //todo ensure i block things!
            PathingGrid.AddBlockingArea(position, 1);
        }

        private void GenerateShrine()
        {
            if (_region.GetRegionType() == RegionType.Shrine)
            {
                RiteShrineBehaviour.Generate(_region.ShrinePosition);
            }

            if (_region.GetRegionType() == RegionType.Fountain)
            {
                FountainBehaviour.Generate(_region.ShrinePosition);
            }

            if (_region.GetRegionType() == RegionType.Monument)
            {
                SaveStoneBehaviour.Generate(_region.ShrinePosition);
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
            Vector2 position = FindAndRemoveValidPosition(3f);
            _region.ShrinePosition = position;
            PathingGrid.AddBlockingArea(position, 3.5f);
            RemoveInvalidPoints();
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

        protected void PlaceFire()
        {
            Vector2 position = FindAndRemoveValidPosition(0.5f);
            _region.Fires.Add(GenerateFire(position));
            CreateImpassablePoint(position);
        }

        protected Vector2 FindAndRemoveValidPosition(float radius = 0, bool ignoreBounds = false)
        {
            for (int i = _availablePositions.Count - 1; i >= 0; --i)
            {
                if (!ignoreBounds && _availablePositions[i].magnitude > PathingGrid.CombatAreaWidth / 2f - radius) continue;
                Cell c = PathingGrid.WorldToCellPosition(_availablePositions[i], false);
                if (c == null) continue;
                if (!c.Reachable) continue;
                if (radius != 0)
                {
                    Vector2 topLeft = new Vector2(_availablePositions[i].x - radius, _availablePositions[i].y - radius);
                    Vector2 bottomRight = new Vector2(_availablePositions[i].x + radius, _availablePositions[i].y + radius);
                    if (!PathingGrid.IsSpaceAvailable(topLeft, bottomRight)) continue;
                }

                _availablePositions.RemoveAt(i);
                return c.Position;
            }

            throw new Exceptions.RegionPositionNotFoundException();
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

        private void GenerateRocks(int number, float minPolyWidth, float maxPolyWidth, float radiusVariation, float smoothness)
        {
            while (number > 0)
            {
                GenerateGenericRock(Random.Range(minPolyWidth, maxPolyWidth), radiusVariation, smoothness, FindAndRemoveValidPosition());
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

        private static int _healthShrineCounter;
        private const int HealShrineTarget = 6;
        private static int _essenceShrineCounter;
        private const int EssenceShrineTarget = 30;

        protected virtual void PlaceItems()
        {
            if (_region._characterHere != null)
            {
                _region.CharacterPosition = FindAndRemoveValidPosition();
                RemoveInvalidPoints();
            }
            PlaceFire();
            //todo tidy me
            if (_healthShrineCounter > HealShrineTarget)
            {
                _region.HealShrinePosition = FindAndRemoveValidPosition();
                _healthShrineCounter = 0;
                RemoveInvalidPoints();
            }

            if (_essenceShrineCounter > EssenceShrineTarget / (EnvironmentManager.CurrentEnvironment.LevelNo + 1))
            {
                _region.EssenceShrinePosition = FindAndRemoveValidPosition();
                _essenceShrineCounter = 0;
                RemoveInvalidPoints();
            }

            _healthShrineCounter += Random.Range(1, 3);
            _essenceShrineCounter += Random.Range(1, 3);

            Helper.Shuffle(_availablePositions);
            for (int i = 0; i < _region.WaterSourceCount; ++i)
            {
                _region.Containers.Add(new WaterSource(FindAndRemoveValidPosition()));
                RemoveInvalidPoints();
            }

            for (int i = 0; i < _region.FoodSourceCount; ++i)
            {
                _region.Containers.Add(new FoodSource(FindAndRemoveValidPosition()));
                RemoveInvalidPoints();
            }

            for (int i = 0; i < _region.ResourceSourceCount; ++i)
            {
                Loot loot = new Loot(FindAndRemoveValidPosition(), "Resource");
                loot.IncrementResource(ResourceTemplate.GetResource().Name, 1);
                _region.Containers.Add(loot);
                RemoveInvalidPoints();
            }
        }

        protected abstract void Generate();
    }
}