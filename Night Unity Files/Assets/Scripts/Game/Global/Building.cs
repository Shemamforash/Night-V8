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
            WorldState.HomeInventory().AddBuilding(this);
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
            if (WeatherManager.CurrentWeather().Name == "Drizzle") WorldState.HomeInventory().IncrementResource("Water", 1);
            else if (WeatherManager.CurrentWeather().Name == "Rain") WorldState.HomeInventory().IncrementResource("Water", 1);
            else if (WeatherManager.CurrentWeather().Name == "Downpour") WorldState.HomeInventory().IncrementResource("Water", 1);
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
            WorldState.HomeInventory().IncrementResource("Meat", 1);
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
            if (WeatherManager.CurrentWeather().Name == "Mist") WorldState.HomeInventory().IncrementResource("Water", 1);
            if (WeatherManager.CurrentWeather().Name == "Fog") WorldState.HomeInventory().IncrementResource("Water", 1);
        }
    }

    public class EssenceFilter : Building
    {
        public EssenceFilter() : base("Essence Filter")
        {
        }

        public override void Update()
        {
            WorldState.HomeInventory().IncrementResource("Essence", 1);
        }
    }
}