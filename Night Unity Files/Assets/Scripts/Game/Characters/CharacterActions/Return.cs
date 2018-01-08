using Game.World;

namespace Game.Characters.CharacterActions
{
    public class Return : BaseCharacterAction
    {
        public Return(Player playerCharacter) : base("Return", playerCharacter)
        {
            IsVisible = false;
            HourCallback = GetCharacter().Return;
            AddOnExit(ReturnToVehicle);
        }

        public override void Enter()
        {
            base.Enter();
            SetDuration(GetCharacter().DistanceFromHome);
            Start();
        }

        private void ReturnToVehicle()
        {
            if (PlayerCharacter.DistanceFromHome == 1)
            {
                GetCharacter().Return();
            }
            GetCharacter().Inventory().MoveAllResources(WorldState.HomeInventory());
        }
    }
}