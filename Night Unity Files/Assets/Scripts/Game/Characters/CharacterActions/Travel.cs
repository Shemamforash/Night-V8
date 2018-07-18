using System;
using Game.Exploration.Environment;
using Game.Exploration.Regions;
using Game.Exploration.WorldEvents;
using Game.Global;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.InventorySystem;

namespace Game.Characters.CharacterActions
{
    public class Travel : BaseCharacterAction
    {
//        private int TimeSpentTravelling;
        private Region _target;
        private Region CurrentNode;
        private bool _inTransit;
        private int _enduranceCost;

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

//                ++TimeSpentTravelling;
//                if (TimeSpentTravelling == WorldState.MinutesPerHour)
//                {
//                    TimeSpentTravelling = 0;
//                }

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
            PlayerCharacter.Tire(_enduranceCost);
            CurrentNode = _target;
            _inTransit = false;
            if (AtHome())
            {
                PlayerCharacter.Attributes.DecreaseWillpower();
//                TimeSpentTravelling = 0;
                foreach (InventoryItem item in PlayerCharacter.Inventory().Contents())
                {
                    if (item.Template == null) continue;
                    switch (item.Template.ResourceType)
                    {
                        case "Water":
                            PlayerCharacter.BrandManager.IncreaseWaterFound();
                            break;
                        case "Plant":
                            PlayerCharacter.BrandManager.IncreaseFoodFound();
                            break;
                        case "Meat":
                            PlayerCharacter.BrandManager.IncreaseFoodFound();
                            break;
                        case "Resource":
                            PlayerCharacter.BrandManager.IncreaseResourceFound();
                            break;
                    }
                }

                PlayerCharacter.Inventory().MoveAllResources(WorldState.HomeInventory());
                PlayerCharacter.RestAction.Enter();
                WorldEventManager.GenerateEvent(new CharacterMessage("I'm back, but the journey has taken it's toll", PlayerCharacter));
            }
            else
            {
                CurrentNode.Discover();
                SceneChanger.ChangeScene("Combat");
            }
        }

//        public int GetTimeRemaining()
//        {
//            int remainingEndurance = (int) PlayerCharacter.Attributes.Val(AttributeType.Endurance);
//            int remainingTime = remainingEndurance * WorldState.MinutesPerHour - TimeSpentTravelling % WorldState.MinutesPerHour;
//            return remainingTime;
//        }

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

        public void TravelTo(Region target, int enduranceCost)
        {
            Enter();
            //todo decide if i want this
//            switch (target.GetRegionType())
//            {
//                case RegionType.Shelter:
//                    WorldEventManager.GenerateEvent(new CharacterMessage("Perhaps I will find others", PlayerCharacter));
//                    break;
//                case RegionType.Gate:
//                    WorldEventManager.GenerateEvent(new CharacterMessage("I'm going back to camp", PlayerCharacter));
//                    break;
//                case RegionType.Temple:
//                    WorldEventManager.GenerateEvent(new CharacterMessage("I hope I will see you again", PlayerCharacter));
//                    break;
//                case RegionType.Animal:
//                    WorldEventManager.GenerateEvent(new CharacterMessage("Perhaps I will find something to hunt", PlayerCharacter));
//                    break;
//                case RegionType.Danger:
//                    WorldEventManager.GenerateEvent(new CharacterMessage("I pray I won't find trouble, but I will be ready if I do", PlayerCharacter));
//                    break;
//                case RegionType.Nightmare:
//                    break;
//                case RegionType.Fountain:
//                    break;
//                case RegionType.Shrine:
//                    break;
//                case 
//                default:
//                    throw new ArgumentOutOfRangeException();
//            }

            _inTransit = true;
            _target = target;
            _enduranceCost = enduranceCost;
            Duration = enduranceCost * WorldState.MinutesPerHour / 2;
        }

        public void SetCurrentNode(Region node)
        {
            CurrentNode = node;
        }
    }
}