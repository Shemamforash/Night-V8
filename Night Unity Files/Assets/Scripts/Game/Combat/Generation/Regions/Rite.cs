using Game.Characters;
using Game.Exploration.Regions;

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