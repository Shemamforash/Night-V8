using UnityEngine;
using Persistence;
using System;

namespace World
{
    public class Home : MonoBehaviour
    {
        private PersistenceListener _persistenceListener;
        private static Resource _water, _food, _fuel;

        public void Awake()
        {
            Func<float, string> litreConversion = f => (Mathf.Round(f * 10f) / 10f).ToString() + "L";
            Func<float, string> foodConversion = f => Mathf.Round(f).ToString() + "kcal";
            Func<float, string> fuelConversion = f => Mathf.Round(f).ToString() + "cans";
            _water = new Resource("Water", litreConversion);
            _food = new Resource("Food", foodConversion);
            _fuel = new Resource("Fuel", fuelConversion);
            _persistenceListener = new PersistenceListener(Load, Save, "Home");
        }

        public static void Load()
        {
            _water.Increment(GameData.storedWater);
            _food.Increment(GameData.storedFood);
            _fuel.Increment(GameData.storedFuel);
        }

        public static void Save()
        {
            GameData.storedWater = _water.Quantity();
            GameData.storedFood = _food.Quantity();
            GameData.storedFuel = _fuel.Quantity();
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
    }
}