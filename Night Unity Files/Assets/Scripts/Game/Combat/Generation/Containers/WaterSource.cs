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
			ResourceItem resource = ResourceTemplate.GetWater().Create();
			if (EnvironmentManager.CurrentEnvironmentType == EnvironmentType.Mountains && resource.Name == "Water")
			{
				resource = ResourceTemplate.Create("Ice");
			}

			_resourceName  = resource.Name;
			Item           = resource;
			PrefabLocation = "Puddle";
			Sprite         = ResourceTemplate.GetSprite("Water");
		}

		protected override string GetLogText() => "Found some " + _resourceName;
	}
}