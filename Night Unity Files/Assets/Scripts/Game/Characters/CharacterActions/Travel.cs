using Game.Combat.Generation;
using Game.Exploration.Environment;
using Game.Exploration.Regions;
using Game.Exploration.WorldEvents;
using Game.Global;
using SamsHelper.BaseGameFunctionality.InventorySystem;

namespace Game.Characters.CharacterActions
{
    public class Travel : BaseCharacterAction
    {
        private Region _target;
        private Region CurrentRegion;
        private bool _inTransit;
        private int _travelTime;
        private const int MinutesPerEndurancePoint = WorldState.MinutesPerHour / 2;

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
                if (_travelTime != MinutesPerEndurancePoint) return;
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
            TransferResources();
        }

        private void TransferResources()
        {
            PlayerCharacter.Attributes.DecreaseWillpower();
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
        }

        private void ReachTarget()
        {
            CurrentRegion = _target;
            _inTransit = false;
            if (AtHome())
            {
                TransferResources();
                PlayerCharacter.RestAction.Enter();
                WorldEventManager.GenerateEvent(new CharacterMessage("I'm back, but the journey has taken it's toll", PlayerCharacter));
            }
            else
            {
               EnterRegion();
            }
        }

        private void EnterRegion()
        {
            bool discovered = CurrentRegion.Discover(PlayerCharacter);
            if(discovered) PlayerCharacter.BrandManager.IncreaseRegionsExplored();
            CombatManager.SetCurrentRegion(CurrentRegion);
            SceneChanger.ChangeScene("Combat");
        }

        protected override void OnClick()
        {
            SceneChanger.ChangeScene("Map");
        }

        public Region GetCurrentNode()
        {
            return CurrentRegion ?? (CurrentRegion = MapGenerator.GetInitialNode());
        }

        public void TravelTo(Region target, int enduranceCost)
        {
            Enter();
            if(target == CurrentRegion && CurrentRegion.GetRegionType() != RegionType.Gate) EnterRegion();
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

            _travelTime = 0;
            _inTransit = true;
            _target = target;
            SetDuration(enduranceCost * MinutesPerEndurancePoint);
        }

        public void ClaimRegion()
        {
            CurrentRegion.ClaimRemaining = 2 * 24 * WorldState.MinutesPerHour;
        }

        public bool InClaimedRegion()
        {
            return CurrentRegion.ClaimRemaining > 0;
        }
    }
}