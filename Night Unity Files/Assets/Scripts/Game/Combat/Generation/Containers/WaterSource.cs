using Game.Exploration.Environment;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using UnityEngine;

namespace Game.Combat.Generation
{
    public class WaterSource : ContainerController
    {
        public WaterSource(Vector2 position) : base(position)
        {
            int capacity = Random.Range(0, 3) + 1;
            Item = ResourceTemplate.GetWater().Create();
            if (EnvironmentManager.BelowFreezing() && Item.Name == "Water")
                Item = ResourceTemplate.Create("Ice");
            Item.Increment(capacity);
            PrefabLocation = "Puddle";
            ImageLocation = "Water";
        }
    }
}