using System;
using System.Collections.Generic;
using Facilitating.Persistence;
using Persistence;
using SamsHelper.Persistence;
using UnityEngine;

namespace Game.World
{
    public class Home : MonoBehaviour
    {
        private PersistenceListener _persistenceListener;
        private static readonly Dictionary<ResourceType, Resource> Resources =
            new Dictionary<ResourceType, Resource>();

        public void Awake()
        {
            Func<float, string> litreConversion = f => (Mathf.Round(f * 10f) / 10f).ToString() + "L";
            Func<float, string> foodConversion = f => Mathf.Round(f).ToString() + "meals";
            Func<float, string> fuelConversion = f => Mathf.Round(f).ToString() + "cans";
            Func<float, string> ammoConversion = f => Mathf.Round(f).ToString() + " rnds";
            Resources[ResourceType.Water] = new Resource("Water", litreConversion);
            Resources[ResourceType.Food] = new Resource("Food", foodConversion);
            Resources[ResourceType.Fuel] = new Resource("Fuel", fuelConversion);
            Resources[ResourceType.Ammo] = new Resource("Ammo", ammoConversion);
            Resources[ResourceType.Ammo].Increment(100);
            _persistenceListener = new PersistenceListener(Load, Save, "Home");
        }

        public static void Load()
        {
            Resources[ResourceType.Water].Increment(GameData.StoredWater);
            Resources[ResourceType.Food].Increment(GameData.StoredFood);
            Resources[ResourceType.Fuel].Increment(GameData.StoredFuel);
        }

        public static void Save()
        {
            GameData.StoredWater = Resources[ResourceType.Water].Quantity();
            GameData.StoredFood = Resources[ResourceType.Food].Quantity();
            GameData.StoredFuel = Resources[ResourceType.Fuel].Quantity();
        }

        public static void IncrementResource(ResourceType resourceType, float amount)
        {
            Resources[resourceType].Increment(amount);
        }

        public static float ConsumeResource(ResourceType resourceType, float amount)
        {
            return Resources[resourceType].Consume(amount);
        }

        public static GameObject GetResourceObject(ResourceType resourceType)
        {
            return Resources[resourceType].GetObject();
        }
    }
}