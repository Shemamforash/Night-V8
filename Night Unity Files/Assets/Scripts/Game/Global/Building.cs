using System;
using System.Xml;
using Extensions;
using Facilitating.Persistence;
using Game.Exploration.Environment;
using Game.Exploration.Weather;
using SamsHelper.BaseGameFunctionality.InventorySystem;

namespace Game.Global
{
	public abstract class Building : NamedItem
	{
		private readonly int _counterTarget;
		private          int _counter;

		protected Building(string name, int counterTarget) : base(name)
		{
			_counterTarget = counterTarget;
			_counter       = _counterTarget;
		}

		public void Update()
		{
			--_counter;
			if (_counter > 0) return;
			_counter = _counterTarget;
			OnUpdate();
		}

		public void OverrideCounter(int counter)
		{
			_counter = counter + 1;
			Update();
		}

		public abstract void OnUpdate();

		protected virtual XmlNode Save(XmlNode root)
		{
			root = root.CreateChild("Building");
			root.CreateChild("Name",    Name);
			root.CreateChild("Counter", _counter);
			return root;
		}

		public static void LoadBuildings(XmlNode root)
		{
			foreach (XmlNode buildingNode in root.SelectSingleNode("Buildings").ChildNodes)
			{
				string   name    = buildingNode.ParseString("Name");
				int      counter = buildingNode.ParseInt("Counter");
				Building building;
				switch (name)
				{
					case "Trap":
						building = new Trap();
						break;
					case "Water Collector":
						building = new WaterCollector();
						break;
					case "Condenser":
						building = new Condenser();
						break;
					case "Essence Filter":
						building = new EssenceFilter();
						break;
					case "Smoker":
						Smoker smoker = new Smoker();
						smoker.Load(buildingNode);
						building = smoker;
						break;
					case "Purifier":
						Purifier purifier = new Purifier();
						purifier.Load(buildingNode);
						building = purifier;
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}

				Inventory.AddBuilding(building);
				building.OverrideCounter(counter);
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
		public WaterCollector() : base("Water Collector", 6)
		{
		}

		public override void OnUpdate()
		{
			string resource = EnvironmentManager.GetTemperature() <= 0 ? "Ice" : "Water";
			if (WeatherManager.CurrentWeather().Attributes.RainAmount > 0) Inventory.IncrementResource(resource, 1);
		}
	}


	public class Trap : Building
	{
		public Trap() : base("Trap", 6)
		{
		}

		public override void OnUpdate()
		{
			Inventory.IncrementResource("Meat", 1);
		}
	}

	public class Condenser : Building
	{
		public Condenser() : base("Condenser", 1)
		{
		}

		public override void OnUpdate()
		{
			if (WeatherManager.CurrentWeather().Attributes.FogAmount == 0) return;
			Inventory.IncrementResource("Water", 1);
		}
	}

	public class EssenceFilter : Building
	{
		public EssenceFilter() : base("Essence Filter", 6)
		{
		}

		public override void OnUpdate()
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
		private readonly int    _targetTime;
		private          int    _time;

		protected Converter(string name, int time, string resourceFrom, string resourceTo) : base(name, 1)
		{
			_targetTime   = time;
			_resourceFrom = resourceFrom;
			_resourceTo   = resourceTo;
		}

		public override void OnUpdate()
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
			_time = root.ParseInt("Time");
		}
	}

	public class Smoker : Converter
	{
		private int _time;

		public Smoker() : base("Smoker", 6, "Rotmeat", "Meat")
		{
		}
	}
}