using Game.Characters;
using Game.Exploration.Regions;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Combat.Generation
{
    public class Rite : RegionGenerator
    {
        private static Brand _brand;
        private static Region _lastRegion;

        public static void SetBrand(Brand brand, Region lastRegion)
        {
            _brand = brand;
            _lastRegion = lastRegion;
        }

        protected override void Generate()
        {
            float angleInterval = 360f / 30f;
            for (float angle = 0; angle < 360; angle += angleInterval)
            {
                Vector2 position = AdvancedMaths.CalculatePointOnCircle(angle, 16, Vector2.zero);
                GenerateOneRock(MinPolyWidth, SmallPolyWidth, 0.5f, 0.75f, position);

                position = AdvancedMaths.CalculatePointOnCircle(angle + 6, 18, Vector2.zero);
                GenerateOneRock(MinPolyWidth, SmallPolyWidth, 0.1f, 0.75f, position);

                position = AdvancedMaths.CalculatePointOnCircle(angle, 21, Vector2.zero);
                GenerateOneRock(MinPolyWidth, SmallPolyWidth, 0.7f, 0.5f, position);
            }
        }

        protected override void PlaceItems()
        {
        }

        protected override void GenerateObjects()
        {
            ShrineBehaviour.Generate(_brand);
        }

        public static Region GetLastRegion()
        {
            return _lastRegion;
        }
    }
}