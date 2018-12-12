using System.Xml;
using Facilitating.Persistence;
using Game.Exploration.Environment;
using Game.Exploration.Weather;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Libraries;
using TriangleNet.Meshing;
using UnityEngine;

namespace Game.Global
{
    public abstract class Building : NamedItem
    {
        protected Building(string name) : base(name)
        {
            Inventory.AddBuilding(this);
        }

        public abstract void Update();

        protected virtual XmlNode Save(XmlNode root)
        {
            root = root.CreateChild("Building");
            root.CreateChild("Name", Name);
            return root;
        }

        public static void LoadBuildings(XmlNode root)
        {
            foreach (XmlNode buildingNode in root.SelectSingleNode("Buildings").ChildNodes)
            {
                string name = buildingNode.StringFromNode("Name");
                switch (name)
                {
                    case "Shelter":
                        Inventory.AddBuilding(new Shelter());
                        break;
                    case "Trap":
                        Inventory.AddBuilding(new Trap());
                        break;
                    case "Water Collector":
                        Inventory.AddBuilding(new WaterCollector());
                        break;
                    case "Condenser":
                        Inventory.AddBuilding(new Condenser());
                        break;
                    case "Essence Filter":
                        Inventory.AddBuilding(new EssenceFilter());
                        break;
                    case "Smoker":
                        Smoker smoker = new Smoker();
                        smoker.Load(root);
                        Inventory.AddBuilding(smoker);
                        break;
                    case "Purifier":
                        Purifier purifier = new Purifier();
                        purifier.Load(root);
                        Inventory.AddBuilding(purifier);
                        break;
                }
            }
        }

        public static void SaveBuildings(XmlNode root)
        {
            XmlNode buildingNode = root.CreateChild("Buildings");
            Inventory.Buildings().ForEach(b => b.Save(buildingNode));
        }
    }

    public class WaterCollector : Building
    {
        public WaterCollector() : base("Water Collector")
        {
        }

        public override void Update()
        {
            string resource = EnvironmentManager.GetTemperature() <= 0 ? "Ice" : "Water";
            if (WeatherManager.CurrentWeather().Attributes.RainAmount > 0) Inventory.IncrementResource(resource, 1);
        }
    }

    public class Shelter : Building
    {
        public Shelter() : base("Shelter")
        {
        }

        public override void Update()
        {
        }
    }

    public class Trap : Building
    {
        private int counter;

        public Trap() : base("Trap")
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
            if (WeatherManager.CurrentWeather().Attributes.FogAmount > 0) Inventory.IncrementResource("Water", 1);
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

        protected override XmlNode Save(XmlNode root)
        {
            root = base.Save(root);
            root.CreateChild("Time", _time);
            return root;
        }

        public void Load(XmlNode root)
        {
            _time = root.IntFromNode("Time");
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