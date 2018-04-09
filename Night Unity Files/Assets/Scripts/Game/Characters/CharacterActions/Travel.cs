using Game.Exploration.Environment;
using Game.Global;
using UnityEngine;

namespace Game.Characters.CharacterActions
{
    public class Travel : BaseCharacterAction
    {
        private int TimeSpentTravelling;
        private MapNode _target;
        private Vector3 TargetPosition;
        private MapNode CurrentNode;
        private bool _inTransit;
        private float JourneyTime;

        public Travel(Player playerCharacter) : base("Travel", playerCharacter)
        {
            DisplayName = "Travelling";
            MinuteCallback = () =>
            {
                if (Duration == 0)
                {
                    if(_inTransit) ReachTarget();
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

        private void ReachTarget()
        {
            CurrentNode = _target;
            _inTransit = false;
            if (CurrentNode.Region == null)
            {
                TimeSpentTravelling = 0;
                PlayerCharacter.Inventory().MoveAllResources(WorldState.HomeInventory());
            }
            else
            {
                CurrentNode.Region.Discover();
                SceneChanger.ChangeScene("Map");
            }
        }

        protected override void OnClick()
        {
            SceneChanger.ChangeScene("Map");
        }

        public MapNode GetCurrentNode()
        {
            return CurrentNode ?? (CurrentNode = MapGenerator.GetInitialNode());
        }

        public bool InTransit()
        {
            return _inTransit;
        }

        public Vector3 GetCurrentPosition()
        {
            if (!_inTransit) return CurrentNode.Position;
            float progress = 1f - Duration / JourneyTime;
            return Vector3.Lerp(CurrentNode.Position, TargetPosition, progress);
        }
        
        public void TravelTo(MapNode target, Vector3 targetPosition)
        {
            Enter();
            _inTransit = true;
            _target = target;
            TargetPosition = targetPosition;
            if (target == null)
            {
                Duration = Random.Range(2, 5);
            }
            else
            {
                float distance = Vector2.Distance(CurrentNode.Position, targetPosition);
                Duration = MapGenerator.NodeDistanceToTime(distance);
                JourneyTime = Duration;
            }
        }
    }
}