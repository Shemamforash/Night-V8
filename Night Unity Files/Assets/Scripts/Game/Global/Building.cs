using Game.Exploration.Weather;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using TriangleNet.Meshing;
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

    public class Purifier : Converter
    {
        private int _time;

        public Purifier() : base("Purifier", 2, "Deathwater", "Water")
        {
        }
    }

    public class Converter : Building
    {
        private readonly string _resourceTo, _resourceFrom;
        private readonly int _targetTime;
        private int _time;

        protected Converter(string name, int time, string resourceFrom, string resourceTo) : base(name)
        {
            _targetTime = time;
            _resourceFrom = resourceFrom;
            _resourceTo = resourceTo;
        }

        public override void Update()
        {
            if (Inventory.GetResourceQuantity(_resourceFrom) == 0)
            {
                _time = 0;
                return;
            }

            ++_time;
            if (_time < _targetTime) return;
            Inventory.DecrementResource(_resourceFrom, 1);
            Inventory.IncrementResource(_resourceTo, 1);
            _time = 0;
        }
    }

    public class Smoker : Converter
    {
        private int _time;

        public Smoker() : base("Smoker", 2, "Rotmeat", "Meat")
        {
        }
    }
}