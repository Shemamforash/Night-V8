using SamsHelper.BaseGameFunctionality.InventorySystem;
using UnityEngine;

namespace Game.Combat.Generation
{
    public class FoodSource : ContainerController
    {
        private GameObject _insectPrefab;

        public FoodSource(Vector2 position) : base(position)
        {
            string foodType = ResourceTemplate.GetPlant().Name;
            ResourceItem resource = ResourceTemplate.Create(foodType);
            Item = resource;
            ImageLocation = "Plants/" + foodType;
        }

        public override ContainerBehaviour CreateObject(bool autoreveal = false)
        {
            ContainerBehaviour container = base.CreateObject(autoreveal);
            if (_insectPrefab == null) _insectPrefab = Resources.Load<GameObject>("Prefabs/Combat/Visuals/Insect");
            GameObject insect = GameObject.Instantiate(_insectPrefab);
            insect.transform.SetParent(container.transform);
            insect.transform.position = container.transform.position;
            container.SetInsect(insect.GetComponent<InsectBehaviour>());
            return container;
        }
    }
}