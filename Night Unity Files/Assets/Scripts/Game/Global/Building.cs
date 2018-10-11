using Game.Exploration.Weather;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using UnityEngine;

namespace Game.Global
{
    public abstract class Building : MyGameObject
    {
        protected Building(string name) : base(name, GameObjectType.Building)
        {
            Inventory.AddBuilding(this);
        }

        public abstract void Update();
    }

    public class WaterCollector : Building
    {
        public WaterCollector() : base("Water Collector")
        {
        }

        public override void Update()
        {
            if (WeatherManager.CurrentWeather().Name == "Drizzle") Inventory.IncrementResource("Water", 1);
            else if (WeatherManager.CurrentWeather().Name == "Rain") Inventory.IncrementResource("Water", 1);
            else if (WeatherManager.CurrentWeather().Name == "Downpour") Inventory.IncrementResource("Water", 1);
        }
    }

    public class Shelter : Building
    {
        public Shelter() : base("Water Collector")
        {
        }

        public override void Update()
        {
        }
    }

    public class Trap : Building
    {
        private int counter;

        public Trap() : base("Water Collector")
        {
            ResetCounter();
        }

        private void ResetCounter()
        {
            counter = Random.Range(2, 5);
        }

        public override void Update()
        {
            --counter;
            if (counter != 0) return;
            Inventory.IncrementResource("Meat", 1);
            ResetCounter();
        }
    }

    public class Condenser : Building
    {
        public Condenser() : base("Condenser")
        {
        }

        public override void Update()
        {
            if (WeatherManager.CurrentWeather().Name == "Mist") Inventory.IncrementResource("Water", 1);
            if (WeatherManager.CurrentWeather().Name == "Fog") Inventory.IncrementResource("Water", 1);
        }
    }

    public class EssenceFilter : Building
    {
        public EssenceFilter() : base("Essence Filter")
        {
        }

        public override void Update()
        {
            Inventory.IncrementResource("Essence", 1);
        }
    }
}