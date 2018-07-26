using System.Collections.Generic;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Combat.Generation
{
    //place walls
    //place shrine
    //place health shrine
    //place fires
    //place items
    //place echo
    //place obstacles
    
    public class Canyon : RegionGenerator
    {
        protected override void Generate()
        {
            Vector2 leftPosition = new Vector2(Random.Range(-7f, -4f), 0);
            Vector2 rightPosition = new Vector2(Random.Range(4f, 7f), 0);
            int rotateAmount = Random.Range(0, 360);
            GenerateRockWall(leftPosition, true, rotateAmount);
            GenerateRockWall(rightPosition, false, rotateAmount);
            PlaceShrine();
            PlaceItems();
            PlaceEchoes();
            GenerateSmallRocks(Random.Range(5, 15));
            GenerateTinyRocks(Random.Range(20, 40));
        }

        private List<Vector2> RockWall(bool left)
        {
            Vector2 start = new Vector2(0, -PathingGrid.CombatAreaWidth * 0.7f);
            Vector2 current = start;

            List<Vector2> points = new List<Vector2>();
            float yStretch = Random.Range(7.5f, 12.5f);
            float verticalOffset = Random.Range(0f, 100f);
            float xStretch = Random.Range(2f, 4f);
            while (current.y < PathingGrid.CombatAreaWidth * 0.7f)
            {
                points.Add(current);
                Vector2 next = current;
                next.x = Mathf.Sin((next.y + verticalOffset) / yStretch) * xStretch;
                next.x -= Mathf.PerlinNoise(next.x, 0) * 2 * Random.Range(1f, 2f);
                next.y += Random.Range(0.5f, 2f);
                current = next;
            }

            if (left)
            {
                points.Add(new Vector2(-PathingGrid.CombatAreaWidth, current.y));
                points.Add(new Vector2(-PathingGrid.CombatAreaWidth, points[0].y));
            }
            else
            {
                points.Add(new Vector2(PathingGrid.CombatAreaWidth, current.y));
                points.Add(new Vector2(PathingGrid.CombatAreaWidth, points[0].y));
                points.Reverse();
            }

            return points;
        }

        private void GenerateRockWall(Vector2 position, bool left, int rotateAmount)
        {
            List<Vector2> wallVertices = RockWall(left);
            for (int i = 0; i < wallVertices.Count; i++) wallVertices[i] = AdvancedMaths.RotatePoint(wallVertices[i], rotateAmount, Vector2.zero);
            position = AdvancedMaths.RotatePoint(position, rotateAmount, Vector2.zero);
            new Barrier(wallVertices, "Wall " + GetObjectNumber(), position, barriers);
        }
    }
}