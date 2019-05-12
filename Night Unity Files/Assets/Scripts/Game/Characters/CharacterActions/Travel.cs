using System.Xml;
using Extensions;
using Facilitating.Persistence;
using Game.Combat.Generation;
using Game.Exploration.Environment;
using Game.Exploration.Regions;
using Game.Global;

namespace Game.Characters.CharacterActions
{
	public class Travel : BaseCharacterAction
	{
		private const int    MinutesPerTravelDistance = WorldState.MinutesPerHour / 2;
		private       bool   _atDestination;
		private       Region _target;
		private       int    _travelTime;
		private       Region CurrentRegion;

		public Travel(Player playerCharacter) : base("Travel", playerCharacter)
		{
			DisplayName = "Travelling";
			MinuteCallback = () =>
			{
				--Duration;
				if (Duration > 0) return;
				if (_target.GetRegionType() == RegionType.Gate)
				{
					ReachTarget();
				}
				else
				{
					_atDestination = true;
				}
			};
		}

		public bool AtDestination => _atDestination;

		public override string GetDisplayName()
		{
			if (_atDestination) return "Arrived At Region";
			return _target.GetRegionType() == RegionType.Gate ? "Returning Home" : base.GetDisplayName();
		}

		public bool AtHome()
		{
			if (CurrentRegion                    == null) return true;
			return CurrentRegion.GetRegionType() == RegionType.Gate;
		}

		public void ReturnToHomeInstant(bool goToGame)
		{
			CurrentRegion = MapGenerator.GetInitialNode();
			PlayerCharacter.RestAction.Enter();
			CombatManager combatManager = CombatManager.Instance();
			if (combatManager != null) combatManager.ExitCombat(false);
			if (goToGame) SceneChanger.GoToGameScene();
		}

		private void ReachTarget()
		{
			CurrentRegion  = _target;
			_atDestination = false;
			if (AtHome())
			{
				PlayerCharacter.RestAction.Enter();
				PlayerCharacter.CharacterView().UpdateActionList();
				PlayerCharacter.CharacterView().ShowAttributeTutorial();
			}
			else
			{
				EnterRegion();
			}
		}

		private void EnterRegion()
		{
			CurrentRegion.Discover();
			CharacterManager.SelectedCharacter = PlayerCharacter;
			CombatStoryController.TryEnter(PlayerCharacter);
		}

		protected override void OnClick()
		{
			if (_atDestination)
			{
				ReachTarget();
			}
			else
			{
				MapMenuController.Open(PlayerCharacter);
			}
		}

		public Region GetCurrentRegion() => CurrentRegion ?? (CurrentRegion = MapGenerator.GetInitialNode());

		public void TravelTo(Region target, int distance)
		{
			_travelTime = 0;
			_target     = target;

			if (target == CurrentRegion)
			{
				if (CurrentRegion.GetRegionType() != RegionType.Gate)
				{
					EnterRegion();
				}
				else
				{
					ReachTarget();
				}

				return;
			}

			int duration                                            = distance * MinutesPerTravelDistance;
			if (target.GetRegionType() == RegionType.Gate) duration /= 2;
			SetDuration(duration);
			ForceViewUpdate = true;
			Enter();
		}

		public void TravelToInstant(Region target)
		{
			_target = target;
			ReachTarget();
		}

		public override XmlNode Load(XmlNode doc)
		{
			doc           = base.Load(doc);
			_target       = MapGenerator.GetRegionById(doc.ParseInt("Target"));
			CurrentRegion = MapGenerator.GetRegionById(doc.ParseInt("CurrentRegion"));
			_travelTime   = doc.ParseInt("TravelTime");
			return doc;
		}

		public override XmlNode Save(XmlNode doc)
		{
			doc = base.Save(doc);
			doc.CreateChild("Target",        _target.RegionID);
			doc.CreateChild("CurrentRegion", CurrentRegion.RegionID);
			doc.CreateChild("TravelTime",    _travelTime);
			return doc;
		}

		public void SetCurrentRegion(Region region) => CurrentRegion = region;
	}
}