using Game.Exploration.Environment;
using Game.Exploration.Regions;
using Game.Global;
using UnityEngine;

namespace Game.Characters.CharacterActions
{
    public class Travel : BaseCharacterAction
    {
        private int TimeSpentTravelling;
        private Region _target;
        private Region CurrentNode;
        private bool _inTransit;

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

                ++TimeSpentTravelling;
                if (TimeSpentTravelling == WorldState.MinutesPerHour)
                {
                    PlayerCharacter.Travel();
                    TimeSpentTravelling = 0;
                }

                --Duration;
            };
        }

        public bool AtHome()
        {
            //todo fix me
            if (CurrentNode == null) return true;
            return CurrentNode.GetRegionType() == RegionType.Gate;
        }

        private void ReachTarget()
        {
            CurrentNode = _target;
            _inTransit = false;
            if (AtHome())
            {
                TimeSpentTravelling = 0;
                PlayerCharacter.Inventory().MoveAllResources(WorldState.HomeInventory());
                PlayerCharacter.RestAction.Enter();
            }
            else
            {
                CurrentNode.Discover();
                CharacterManager.SelectedCharacter.TravelAction.GetCurrentNode().Enter();
            }
        }

        protected override void OnClick()
        {
            SceneChanger.ChangeScene("Map");
        }

        public Region GetCurrentNode()
        {
            return CurrentNode ?? (CurrentNode = MapGenerator.GetInitialNode());
        }

        public bool InTransit()
        {
            return _inTransit;
        }

        public void TravelTo(Region target, int duration)
        {
            Enter();
            _inTransit = true;
            _target = target;
            Duration = duration;
        }

        public void SetCurrentNode(Region node)
        {
            CurrentNode = node;
        }
    }
}