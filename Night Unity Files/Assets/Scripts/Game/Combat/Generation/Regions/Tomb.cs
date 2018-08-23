using UnityEngine;

namespace Game.Combat.Generation
{
    public class Tomb : RegionGenerator //not a mine
    {
        protected override void Generate()
        {
            GameObject prefab = Resources.Load<GameObject>("Prefabs/Combat/Tomb Portal");
            Instantiate(prefab).transform.position = Vector2.zero;
        }
    }
}