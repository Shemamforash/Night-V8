using System.Collections.Generic;
using Game.Combat.Misc;
using Game.Exploration.Regions;
using NUnit.Framework;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Combat.Generation
{
    public abstract class RegionGenerator : MonoBehaviour
    {
        private const float MinPolyWidth = 0.1f, SmallPolyWidth = 0.2f, MediumPolyWidth = 2f, LargePolyWidth = 3f, HugePolyWidth = 4f;
        private Region _region;
        private List<Vector2> __availablePositions;
        private List<Vector2> _availablePositions;
        private int _barrierNumber;
        protected readonly List<Barrier> barriers = new List<Barrier>();

        public void Initialise(Region region)
        {
            _region = region;
            Random.InitState(region.RegionID);
            if (!_region.Visited())
            {
                GenerateFreshEnvironment();
                _region.Visit();
                return;
            }

            _region.Visit();
            PathingGrid.InitialiseGrid();
            _region.Fires.ForEach(f => f.CreateObject());
            _region.Containers.ForEach(c => c.CreateObject());
            _region.Barriers.ForEach(b => b.CreateObject());
            PathingGrid.FinaliseGrid();
        }

        private void GenerateFreshEnvironment()
        {
            __availablePositions = AdvancedMaths.GetPoissonDiscDistribution(1000, 1f, 3f, PathingGrid.CombatAreaWidth / 2f);
            _availablePositions = new List<Vector2>(__availablePositions);
            PathingGrid.InitialiseGrid();
            Generate();
            foreach (Barrier barrier in barriers)
            {
                if (!barrier.Valid) continue;
                barrier.CreateObject();
                _region.Barriers.Add(barrier);
            }

            _region.Barriers = barriers;
            PlaceShrines();
            PlaceItems();
            _region.Fires.ForEach(f => f.CreateObject());
            _region.Containers.ForEach(c => c.CreateObject());
            PathingGrid.FinaliseGrid();
        }

        private void PlaceShrines()
        {
            for (int i = 0; i < _availablePositions.Count; ++i)
            {
                Vector2 topLeft = new Vector2(_availablePositions[i].x - 3f, _availablePositions[i].y - 3f);
                Vector2 bottomRight = new Vector2(_availablePositions[i].x + 3f, _availablePositions[i].y + 3f);
                if (_availablePositions[i].magnitude > 20) continue;
                if (PathingGrid.WorldToCellPosition(_availablePositions[i]) == null) continue;
                if (!PathingGrid.IsSpaceAvailable(topLeft, bottomRight)) continue;
                _availablePositions.RemoveAt(i);
                return;
            }
        }

        private void GenerateGenericRock(float scale, float radiusVariation, float smoothness, Vector2? position = null)
        {
            smoothness = Mathf.Clamp(1 - smoothness, 0.01f, 1f);
            radiusVariation = 1 - Mathf.Clamp(radiusVariation, 0f, 1f);

            Assert.IsTrue(radiusVariation <= 1);
            Assert.IsTrue(smoothness <= 1);

            float definition = smoothness * 150f;
            radiusVariation *= scale;
            float minX = Random.Range(0, radiusVariation);
            float minY = Random.Range(0, radiusVariation);
            float maxX = Random.Range(minX, scale);
            float maxY = Random.Range(minY, scale);
//            Ellipse e = new Ellipse(minX, minY, maxX, maxY);
            Ellipse e = new Ellipse(radiusVariation, scale);
            float angleIncrement;
            List<Vector2> barrierVertices = new List<Vector2>();
            for (float i = 0; i < 360; i += angleIncrement)
            {
                Vector2 vertex = RandomPointBetweenRadii(i, e);
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
                Cell c = PathingGrid.WorldToCellPosition(_availablePositions[i]);
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

        private void AssignRockPosition(List<Vector2> barrierVertices, Vector2? position)
        {
            if (position == null) position = FindAndRemoveValidPosition();
            if (position == null) return;
            Barrier barrier = new Barrier(barrierVertices, "Barrier " + GetObjectNumber(), (Vector2) position);
            barriers.Add(barrier);
        }

        private static Vector2 RandomPointBetweenRadii(float angle, Ellipse e)
        {
            Vector2 randomPoint;
            angle *= Mathf.Deg2Rad;
            if (e.IsCircle)
            {
                randomPoint = new Vector2();
                float pointRadius = Random.Range(e.InnerRingWidth, e.OuterRingWidth);
                randomPoint.x = pointRadius * Mathf.Cos(angle);
                randomPoint.y = pointRadius * Mathf.Sin(angle);
                return randomPoint;
            }

            Vector2 innerRadiusPoint = new Vector2();
            innerRadiusPoint.x = e.InnerRingWidth * Mathf.Cos(angle);
            innerRadiusPoint.y = e.InnerRingHeight * Mathf.Sin(angle);
            Vector2 outerRadiusPoint = new Vector2();
            outerRadiusPoint.x = e.OuterRingWidth * Mathf.Cos(angle);
            outerRadiusPoint.y = e.OuterRingHeight * Mathf.Sin(angle);

            randomPoint = outerRadiusPoint - innerRadiusPoint;
            randomPoint *= Random.Range(0f, 1f);
            randomPoint += innerRadiusPoint;
            return randomPoint;
        }

        private void GenerateRocks(int number, float minPolyWidth, float maxPolyWidth, float radiusVariation, float smoothness)
        {
            while (number > 0)
            {
                GenerateGenericRock(Random.Range(minPolyWidth, maxPolyWidth), radiusVariation, smoothness);
                --number;
            }
        }

        protected void GenerateHugeRocks(int number = 1)
        {
            GenerateRocks(number, LargePolyWidth, HugePolyWidth, 0.25f, 0.75f);
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
            Helper.Shuffle(ref _availablePositions);
            for (int i = 0; i < 4; ++i)
            {
                Vector2? position = FindAndRemoveValidPosition();
                if (position == null) return;
                _region.Containers.Add(new FoodSource(position.Value));
                position = FindAndRemoveValidPosition();
                if (position == null) return;
                _region.Containers.Add(new WaterSource(position.Value));
            }
        }

        protected abstract void Generate();

        private class Ellipse
        {
            public readonly float InnerRingWidth, InnerRingHeight, OuterRingWidth, OuterRingHeight;
            public readonly bool IsCircle;

            public Ellipse(float innerRingWidth, float innerRingHeight, float outerRingWidth, float outerRingHeight)
            {
                InnerRingWidth = innerRingWidth;
                InnerRingHeight = innerRingHeight;
                OuterRingWidth = outerRingWidth;
                OuterRingHeight = outerRingHeight;
            }

            public Ellipse(float innerRadius, float outerRadius) : this(innerRadius, innerRadius, outerRadius, outerRadius)
            {
                IsCircle = true;
            }
        }
    }
}