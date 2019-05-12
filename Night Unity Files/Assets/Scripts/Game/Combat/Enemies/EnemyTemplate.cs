using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Extensions;
using SamsHelper;

namespace Game.Combat.Enemies
{
	public class EnemyTemplate
	{
		private static          bool                                 _loaded;
		private static readonly Dictionary<EnemyType, EnemyTemplate> EnemyTemplates = new Dictionary<EnemyType, EnemyTemplate>();
		private static readonly List<EnemyType>                      _enemyTypes    = new List<EnemyType>();
		public readonly         string                               DisplayName;
		public readonly         float                                DropRate;
		public readonly         string                               DropResource, Species;
		public readonly         EnemyType                            EnemyType;
		public readonly         bool                                 HasWeapon;
		public readonly         int                                  Health, Speed, Value, Difficulty;

		private EnemyTemplate(XmlNode enemyNode)
		{
			EnemyType                 = StringToType(enemyNode.ParseString("Name"));
			Health                    = enemyNode.ParseInt("Health");
			Speed                     = enemyNode.ParseInt("Speed");
			Value                     = enemyNode.ParseInt("Value");
			Difficulty                = enemyNode.ParseInt("Difficulty");
			HasWeapon                 = enemyNode.ParseBool("HasWeapon");
			Species                   = enemyNode.ParseString("Species");
			DisplayName               = enemyNode.ParseString("DisplayName");
			EnemyTemplates[EnemyType] = this;
			DropResource              = enemyNode.ParseString("Drops");
			DropRate                  = enemyNode.ParseFloat("DropRate");
		}

		public static List<EnemyTemplate> GetEnemyTypes()
		{
			LoadTemplates();
			return EnemyTemplates.Values.ToList();
		}

		private static EnemyType StringToType(string typeName)
		{
			if (_enemyTypes.Count == 0)
			{
				foreach (EnemyType enemyType in Enum.GetValues(typeof(EnemyType))) _enemyTypes.Add(enemyType);
			}

			foreach (EnemyType enemyType in _enemyTypes)
			{
				if (enemyType.ToString() == typeName)
				{
					return enemyType;
				}
			}

			throw new Exceptions.EnemyTypeDoesNotExistException(typeName);
		}

		private static void LoadTemplates()
		{
			if (_loaded) return;
			XmlNode root = Helper.OpenRootNode("Enemies", "Enemies");
			foreach (XmlNode enemyNode in root.GetNodesWithName("Enemy"))
				new EnemyTemplate(enemyNode);
			_loaded = true;
		}

		private static EnemyTemplate GetEnemyTemplate(EnemyType enemyType)
		{
			LoadTemplates();
			if (!EnemyTemplates.ContainsKey(enemyType)) throw new Exceptions.EnemyTypeDoesNotExistException(enemyType.ToString());
			return EnemyTemplates[enemyType];
		}

		public static EnemyBehaviour Create(EnemyType enemyType) => new Enemy(GetEnemyTemplate(enemyType)).GetEnemyBehaviour();

		public static List<EnemyType> RandomiseEnemiesToSize(List<EnemyType> validTypes, int size)
		{
			List<EnemyType> enemyTypes = new List<EnemyType>();
			while (size > 0)
			{
				validTypes.Shuffle();
				foreach (EnemyType e in validTypes)
				{
					int enemyValue = GetEnemyTemplate(e).Value;
					if (enemyValue > size) continue;
					enemyTypes.Add(e);
					size -= enemyValue;
					break;
				}
			}

			return enemyTypes;
		}

		public static int GetEnemyValue(EnemyType enemyType) => GetEnemyTemplate(enemyType).Value;
	}
}