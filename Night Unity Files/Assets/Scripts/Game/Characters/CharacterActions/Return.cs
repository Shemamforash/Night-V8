using Game.World;

namespace Game.Characters.CharacterActions
{
    public class Return : BaseCharacterAction
    {
        public Return(Player playerCharacter) : base("Return", playerCharacter)
        {
            if(playerCharacter.DistanceFromHome == 0) Exit();
            IsVisible = false;
            HourCallback = GetCharacter().Return;
            AddOnExit(ReturnToVehicle);
        }

        public override void Enter()
        {
            SetDuration(GetCharacter().DistanceFromHome);
            Start();
        }

        private void ReturnToVehicle()
        {
            GetCharacter().Return();
            GetCharacter().Inventory().MoveAllResources(WorldState.HomeInventory());
        }
    }
}