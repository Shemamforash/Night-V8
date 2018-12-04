using System.Xml;
using Facilitating.Persistence;
using Game.Combat.Generation;
using Game.Exploration.Environment;
using Game.Exploration.Regions;
using Game.Global;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.MenuSystem;
using UnityEngine;
using UnityEngine.Assertions;

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

        public override string GetDisplayName()
        {
            return _target.GetRegionType() == RegionType.Gate ? "Returning Home" : base.GetDisplayName();
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
            _inTransit = false;
            CurrentRegion = _target;
            if (AtHome())
            {
                TutorialManager.TryOpenTutorial(6);
                PlayerCharacter.RestAction.Enter();
            }
            else
            {
                EnterRegion();
            }
        }

        private void EnterRegion()
        {
            if (CurrentRegion.Discover()) PlayerCharacter.BrandManager.IncreaseRegionsExplored();
            CombatManager.SetCurrentRegion(CurrentRegion);
            CharacterManager.SelectedCharacter = PlayerCharacter;
            SceneChanger.GoToCombatScene();
        }

        protected override void OnClick()
        {
            MenuStateMachine.ShowMenu("Map Menu");
        }

        public Region GetCurrentRegion()
        {
            return CurrentRegion ?? (CurrentRegion = MapGenerator.GetInitialNode());
        }

        public void TravelTo(Region target, int gritCost)
        {
            target.ShouldGenerateEncounter = target != CurrentRegion;
            if (target == CurrentRegion && CurrentRegion.GetRegionType() != RegionType.Gate)
            {
                EnterRegion();
                return;
            }
            Enter();
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

        public override XmlNode Load(XmlNode doc)
        {
            doc = base.Load(doc);
            _target = MapGenerator.GetRegionById(doc.IntFromNode("Target"));
            CurrentRegion = MapGenerator.GetRegionById(doc.IntFromNode("CurrentRegion"));
            _inTransit = true;
            _travelTime = doc.IntFromNode("TravelTime");
            return doc;
        }

        public override XmlNode Save(XmlNode doc)
        {
            doc = base.Save(doc);
            doc.CreateChild("Target", _target.RegionID);
            doc.CreateChild("CurrentRegion", CurrentRegion.RegionID);
            doc.CreateChild("TravelTime", _travelTime);
            return doc;
        }
    }
}