namespace Game.Combat.Generation
{
    public class Forest : RegionGenerator
    {
        protected override void PlaceItems()
        {
            base.PlaceItems();
        }

        protected override void Generate()
        {
            GenerateMediumRocks(5);
            GenerateSmallRocks(50);
            GenerateTinyRocks(200);
            PlaceFire();
        }
    }
}