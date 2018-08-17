﻿using Game.Characters;

namespace Game.Combat.Generation
{
    public class Rite : RegionGenerator
    {
        private static BrandManager.Brand _brand;
        
        public static void SetBrand(BrandManager.Brand brand)
        {
            _brand = brand;
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
    }
}