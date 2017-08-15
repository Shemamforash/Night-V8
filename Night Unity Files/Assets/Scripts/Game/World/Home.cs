using UnityEngine;
using Persistence;
using System;
using Facilitating.Persistence;
using UnityEngine.UI;

namespace World
{
    public class Home : MonoBehaviour
    {
        private PersistenceListener _persistenceListener;
        private static Resource _water, _food, _fuel, _ammo;

        public void Awake()
        {
            Func<float, string> litreConversion = f => (Mathf.Round(f * 10f) / 10f).ToString() + "L";
            Func<float, string> foodConversion = f => Mathf.Round(f).ToString() + "meals";
            Func<float, string> fuelConversion = f => Mathf.Round(f).ToString() + "cans";
            Func<float, string> ammoConversion = f => Mathf.Round(f).ToString() + " rnds";
            _water = new Resource("Water", litreConversion);
            _food = new Resource("Food", foodConversion);
            _fuel = new Resource("Fuel", fuelConversion);
            _ammo = new Resource("Ammo", ammoConversion);
            _ammo.Increment(100);
            _persistenceListener = new PersistenceListener(Load, Save, "Home");
        }

        public static void Load()
        {
            _water.Increment(GameData.StoredWater);
            _food.Increment(GameData.StoredFood);
            _fuel.Increment(GameData.StoredFuel);
        }

        public static void Save()
        {
            GameData.StoredWater = _water.Quantity();
            GameData.StoredFood = _food.Quantity();
            GameData.StoredFuel = _fuel.Quantity();
        }

        public static void IncrementResource(Resource.ResourceType resourceType, float amount)
        {
            switch (resourceType)
            {
                case Resource.ResourceType.Water:
                    _water.Increment(amount);
                    break;
                case Resource.ResourceType.Fuel:
                    _fuel.Increment(amount);
                    break;
                case Resource.ResourceType.Food:
                    _food.Increment(amount);
                    break;
            }
        }

        public static float ConsumeResource(Resource.ResourceType resourceType, float amount)
        {
            switch (resourceType)
            {
                case Resource.ResourceType.Water:
                    return _water.Consume(amount);
                case Resource.ResourceType.Fuel:
                    return _fuel.Consume(amount);
                case Resource.ResourceType.Food:
                    return _food.Consume(amount);
                default:
                    return 0;
            }
        }

        public static GameObject GetResourceObject(Resource.ResourceType type)
        {
            switch (type)
            {
                case Resource.ResourceType.Water:
                    return _water.GetObject();
                case Resource.ResourceType.Food:
                    return _food.GetObject();
                case Resource.ResourceType.Fuel:
                    return _fuel.GetObject();
                default:
                    return null;
            }
        }
    }
}