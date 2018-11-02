using System.Xml;
using Facilitating.Persistence;
using Game.Combat.Generation;
using Game.Exploration.Environment;
using Game.Exploration.Regions;
using Game.Exploration.WorldEvents;
using Game.Global;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.MenuSystem;

namespace Game.Characters.CharacterActions
{
    public class Travel : BaseCharacterAction
    {
        private Region _target;
        private Region CurrentRegion;
        private bool _inTransit;
        private int _travelTime;
        private const int MinutesPerGritPoint = WorldState.MinutesPerHour / 2;

        public Travel(Player playerCharacter) : base("Travel", playerCharacter)
        {
            DisplayName = "Travelling";
            MinuteCallback = () =>
            {
                if (Duration == 0)
                {
                    if (_inTransit) ReachTarget();
                    return;
                }

                --Duration;
                ++_travelTime;
                if (_travelTime != MinutesPerGritPoint) return;
                _travelTime = 0;
                if (_target.GetRegionType() == RegionType.Gate) return;
                playerCharacter.Tire();
            };
        }

        public bool AtHome()
        {
            if (CurrentRegion == null) return true;
            return CurrentRegion.GetRegionType() == RegionType.Gate;
        }

        public void ReturnToHomeInstant()
        {
            CurrentRegion = MapGenerator.GetInitialNode();
            _inTransit = false;
            PlayerCharacter.RestAction.Enter();
        }

        private void ReachTarget()
        {
            CurrentRegion = _target;
            _inTransit = false;
            if (AtHome())
            {
                TutorialManager.TryOpenTutorial(4, 1f);
                PlayerCharacter.RestAction.Enter();
            }
            else
            {
                EnterRegion();
            }
        }

        private void EnterRegion()
        {
            bool justDiscovered = CurrentRegion.Discover();
            if (justDiscovered) PlayerCharacter.BrandManager.IncreaseRegionsExplored();
            CombatManager.SetCurrentRegion(CurrentRegion);
            SceneChanger.GoToCombatScene();
        }

        protected override void OnClick()
        {
            MenuStateMachine.ShowMenu("Map Menu");
        }

        public Region GetCurrentNode()
        {
            return CurrentRegion ?? (CurrentRegion = MapGenerator.GetInitialNode());
        }

        public void TravelTo(Region target, int gritCost)
        {
            Enter();
            if (target == CurrentRegion && CurrentRegion.GetRegionType() != RegionType.Gate) EnterRegion();
            _travelTime = 0;
            _inTransit = true;
            _target = target;
            SetDuration(gritCost * MinutesPerGritPoint);
        }

        public void TravelToInstant(Region target)
        {
            _target = target;
            ReachTarget();
        }

        public bool InClaimedRegion()
        {
            return CurrentRegion.ClaimRemaining > 0;
        }

        public override XmlNode Load(XmlNode doc)
        {
            doc = base.Save(doc);
            _target = MapGenerator.GetRegionById(doc.IntFromNode("Target"));
            CurrentRegion = MapGenerator.GetRegionById(doc.IntFromNode("CurrentRegion"));
            _inTransit = doc.BoolFromNode("InTransit");
            _travelTime = doc.IntFromNode("TravelTime");
            if (!_inTransit && !AtHome()) SaveController.ResumeInCombat = this;
            return doc;
        }

        public override XmlNode Save(XmlNode doc)
        {
            doc = base.Save(doc);
            doc.CreateChild("Target", _target.RegionID);
            doc.CreateChild("CurrentRegion", CurrentRegion.RegionID);
            doc.CreateChild("InTransit", _inTransit);
            doc.CreateChild("TravelTime", _travelTime);
            return doc;
        }
    }
}