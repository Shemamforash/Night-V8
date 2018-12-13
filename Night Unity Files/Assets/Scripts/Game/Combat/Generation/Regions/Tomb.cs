using Game.Exploration.Environment;
using UnityEngine;

namespace Game.Combat.Generation
{
    public class Tomb : RegionGenerator //not a mine
    {
        private static GameObject _tombPrefab;

        protected override void Generate()
        {
            if (EnvironmentManager.CurrentEnvironment.EnvironmentType == EnvironmentType.Wasteland)
            {
                //todo journals, enemies
                return;
            }

            if (_tombPrefab == null) _tombPrefab = Resources.Load<GameObject>("Prefabs/Combat/Tomb Portal");
            Instantiate(_tombPrefab).transform.position = Vector2.zero;
        }
    }
}