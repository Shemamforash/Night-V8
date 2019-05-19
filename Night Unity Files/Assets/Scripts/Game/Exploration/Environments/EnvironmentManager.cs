using System;
using System.Collections.Generic;
using System.Xml;
using Facilitating.Persistence;
using Game.Characters;
using Game.Exploration.Weather;
using Game.Global;
using Extensions;

namespace Game.Exploration.Environment
{
	public static class EnvironmentManager
	{
		private static          bool                                     _loaded;
		private static readonly Dictionary<EnvironmentType, Environment> _environments = new Dictionary<EnvironmentType, Environment>();
		private static          Environment                              _currentEnvironment;
		private static          EnvironmentType                          _currentEnvironmentType;

		public static Environment CurrentEnvironment => _currentEnvironment;

		public static EnvironmentType CurrentEnvironmentType
		{
			get => _currentEnvironmentType;
			private set => _currentEnvironmentType = value;
		}

		public static void Start()
		{
			WorldView.SetEnvironmentText(Environment.EnvironmentTypeToName(_currentEnvironment.EnvironmentType));
			SceneryController.UpdateEnvironmentBackground();
		}

		public static void Reset(bool isLoading)
		{
			NextLevel(true, isLoading);
		}

		public static void NextLevel(bool reset, bool isLoading)
		{
			LoadEnvironments();
			if (reset)
			{
				CurrentEnvironmentType = EnvironmentType.Desert;
			}
			else
			{
				switch (_currentEnvironment.EnvironmentType)
				{
					case EnvironmentType.Desert:
						CurrentEnvironmentType = EnvironmentType.Mountains;
						AchievementManager.Instance().CompleteActI();
						break;
					case EnvironmentType.Mountains:
						CurrentEnvironmentType = EnvironmentType.Sea;
						AchievementManager.Instance().CompleteActII();
						break;
					case EnvironmentType.Sea:
						CurrentEnvironmentType = EnvironmentType.Ruins;
						AchievementManager.Instance().CompleteActIII();
						break;
					case EnvironmentType.Ruins:
						CurrentEnvironmentType = EnvironmentType.Wasteland;
						AchievementManager.Instance().CompleteActIV();
						break;
					case EnvironmentType.Wasteland:
						CurrentEnvironmentType = EnvironmentType.End;
						AchievementManager.Instance().CompleteActV();
						if (CharacterManager.AlternateCharacter != null)
						{
							AchievementManager.Instance().KeepCompanionAlive();
						}

						return;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}

			SetCurrentEnvironment();
			if (!isLoading && _currentEnvironment != null) MapGenerator.Generate();
		}

		private static void SetCurrentEnvironment()
		{
			LoadEnvironments();
			if (CurrentEnvironmentType == EnvironmentType.End)
			{
				_currentEnvironment = null;
				return;
			}

			_currentEnvironment = _environments[CurrentEnvironmentType];
		}

		private static void LoadEnvironments()
		{
			if (_loaded) return;
			XmlNode root = Helper.OpenRootNode("Environments", "EnvironmentTypes");
			foreach (XmlNode environmentNode in root.ChildNodes)
			{
				Environment environment = new Environment(environmentNode);
				_environments.Add(environment.EnvironmentType, environment);
			}

			_loaded = true;
		}

		public static void Save(XmlNode doc)
		{
			if (_currentEnvironment == null) return;
			doc.CreateChild("CurrentEnvironment", CurrentEnvironmentType.ToString());
		}

		public static void Load(XmlNode doc)
		{
			LoadEnvironments();
			string currentEnvironmentText = doc.ParseString("CurrentEnvironment");
			foreach (EnvironmentType environmentType in Enum.GetValues(typeof(EnvironmentType)))
			{
				if (environmentType.ToString() != currentEnvironmentText) continue;
				CurrentEnvironmentType = environmentType;
				SetCurrentEnvironment();
				break;
			}
		}
	}
}