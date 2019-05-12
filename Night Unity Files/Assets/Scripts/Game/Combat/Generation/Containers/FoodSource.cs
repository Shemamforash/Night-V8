using SamsHelper.BaseGameFunctionality.InventorySystem;
using UnityEngine;

namespace Game.Combat.Generation
{
	public class FoodSource : ContainerController
	{
		private GameObject _insectPrefab;

		public FoodSource(Vector2 position) : base(position)
		{
			string       plantType = ResourceTemplate.GetPlant().Name;
			ResourceItem resource  = ResourceTemplate.Create(plantType);
			Item   = resource;
			Sprite = ResourceTemplate.GetSprite(plantType);
		}

		public override ContainerBehaviour CreateObject(bool autoReveal = false)
		{
			ContainerBehaviour container             = base.CreateObject(autoReveal);
			if (_insectPrefab == null) _insectPrefab = Resources.Load<GameObject>("Prefabs/Combat/Visuals/Insect");
			GameObject insect                        = Object.Instantiate(_insectPrefab, container.transform, true);
			Transform  containerTransform            = container.transform;
			insect.transform.parent   = containerTransform;
			insect.transform.position = containerTransform.position;
			container.SetInsect(insect.GetComponent<InsectBehaviour>());
			return container;
		}

		protected override string GetLogText() => "Found a plant";
	}
}