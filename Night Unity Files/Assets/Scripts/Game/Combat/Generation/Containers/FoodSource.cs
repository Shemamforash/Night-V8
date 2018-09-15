using SamsHelper.BaseGameFunctionality.InventorySystem;
using UnityEngine;

namespace Game.Combat.Generation
{
    public class FoodSource : ContainerController
    {
        private readonly string _foodType;
        private GameObject _insectPrefab;

        public FoodSource(Vector2 position) : base(position, "Plant")
        {
            _foodType = ResourceTemplate.GetPlant().Name;
            _inventory.Name = _foodType;
            _inventory.IncrementResource(_foodType, 1);
            ImageLocation = "Plants/" + _foodType;
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