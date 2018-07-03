using System.Collections.Generic;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Combat.Generation
{
    public class Steppe : RegionGenerator
    {
        private List<Vector2> _verts;

        protected override void Generate()
        {
            for (int i = 0; i < 50; ++i)
            {
                _verts = new List<Vector2>();
                Vector2 start = new Vector2(-Random.Range(4, 8), 0);
                Vector2 end = new Vector2(Random.Range(4, 8), 0);
                _verts.Add(start);
                AddVerts(start, end, 1);
                _verts.Add(end);
                AddVerts(end, start, -1);
                AssignRockPosition(_verts, null);
            }
        }

        private void AddVerts(Vector2 start, Vector2 end, int polarity)
        {
            List<float> positionsOnLine = GetRandomPositions();
            positionsOnLine.ForEach(p =>
            {
                float xPos = AdvancedMaths.PointAlongLine(start, end, p).x;
                float height = polarity * (Mathf.PerlinNoise(p, 0) * 2f - 1f);
                Vector2 pos = new Vector2(xPos, height);
                _verts.Add(pos);
            });
        }

        private static List<float> GetRandomPositions()
        {
            List<float> positionsOnLine = new List<float>();
            for (int j = 0; j < Random.Range(10, 20); ++j)
            {
                positionsOnLine.Add(Random.Range(0.01f, 0.99f));
            }

            positionsOnLine.Sort((a, b) => a.CompareTo(b));
            return positionsOnLine;
        }
    }
}