using UnityEngine;

namespace Game.Combat.Generation
{
    public class Forest : RegionGenerator
    {
        protected override void Generate()
        {
            PlaceShrine();
            PlaceItems();
            PlaceEchoes();
            GenerateMediumRocks(Random.Range(2, 6));
            GenerateSmallRocks(Random.Range(20, 30));
            GenerateTinyRocks(Random.Range(30, 50));
        }
    }
}