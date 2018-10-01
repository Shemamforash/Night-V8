using UnityEngine;

namespace Game.Combat.Generation
{
    public class Desert : RegionGenerator
    {
        protected override void Generate()
        {
            PlaceShrine();
            PlaceItems();
            GenerateMediumRocks(Random.Range(2, 10));
            GenerateSmallRocks(Random.Range(20, 45));
            GenerateTinyRocks(Random.Range(30, 90));
        }
    }
}