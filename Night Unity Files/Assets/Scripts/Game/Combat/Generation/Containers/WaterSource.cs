using Game.Exploration.Environment;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using UnityEngine;

namespace Game.Combat.Generation
{
	public class WaterSource : ContainerController
	{
		private readonly string _resourceName;

		public WaterSource(Vector2 position) : base(position)
		{
			int          capacity = Random.Range(0, 2);
			ResourceItem resource = ResourceTemplate.GetWater().Create();
			if (EnvironmentManager.BelowFreezing() && resource.Name == "Water")
			{
				resource = ResourceTemplate.Create("Ice");
			}

			_resourceName = resource.Name;
			resource.Increment(capacity);
			Item           = resource;
			PrefabLocation = "Puddle";
			Sprite         = ResourceTemplate.GetSprite("Water");
		}

		protected override string GetLogText() => "Found some " + _resourceName;
	}
}