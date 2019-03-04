using System.Xml;
using Facilitating.Persistence;
using Game.Combat.Generation;
using Game.Exploration.Environment;
using Game.Exploration.Regions;
using Game.Global;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.MenuSystem;
using UnityEngine;

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
            CombatManager combatManager = CombatManager.Instance();
            if (combatManager != null) combatManager.ExitCombat(false);
            SceneChanger.GoToGameScene();
        }

        private void ReachTarget()
        {
            _inTransit = false;
            _target.ShouldGenerateEncounter = _target != CurrentRegion;
            CurrentRegion = _target;
            if (AtHome())
            {
                PlayerCharacter.CharacterView().ShowAttributeTutorial();
                PlayerCharacter.RestAction.Enter();
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
            CombatStoryController.TryEnter();
        }

        protected override void OnClick()
        {
            MenuStateMachine.ShowMenu("Map Menu");
        }

        public Region GetCurrentRegion()
        {
            return CurrentRegion ?? (CurrentRegion = MapGenerator.GetInitialNode());
        }

        public void TravelTo(Region target, int distance)
        {
            if (target == CurrentRegion && CurrentRegion.GetRegionType() != RegionType.Gate)
            {
                EnterRegion();
                return;
            }

            _travelTime = 0;
            _inTransit = true;
            _target = target;
            int duration = distance * MinutesPerGritPoint;
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
            doc = base.Load(doc);
            _target = MapGenerator.GetRegionById(doc.IntFromNode("Target"));
            CurrentRegion = MapGenerator.GetRegionById(doc.IntFromNode("CurrentRegion"));
            _inTransit = true;
            _travelTime = doc.IntFromNode("TravelTime");
            _target.ShouldGenerateEncounter = _target != CurrentRegion;
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

        public void SetCurrentRegion(Region region) => CurrentRegion = region;
    }
}