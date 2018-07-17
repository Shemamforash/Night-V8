using UnityEngine;

namespace Game.Combat.Generation
{
    public class Temple : RegionGenerator
    {
        protected override void Generate()
        {
            GameObject temple = Instantiate(Resources.Load<GameObject>("Prefabs/Combat/Buildings/Temple"));
            temple.transform.position = Vector2.zero;
        }

        protected override void PlaceItems()
        {
        }
    }
}