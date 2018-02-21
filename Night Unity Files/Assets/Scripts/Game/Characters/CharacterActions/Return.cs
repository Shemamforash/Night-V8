using Game.World;
using UnityEngine.Assertions;

namespace Game.Characters.CharacterActions
{
    public class Return : BaseCharacterAction
    {
        public Return(Player.Player playerCharacter) : base("Return", playerCharacter)
        {
            IsVisible = false;
            HourCallback = () =>
            {
                --Duration;
                PlayerCharacter.Return();
                if (Duration != 0) return;
                ReturnToVehicle();
                Exit();
            };
        }

        public override void Enter()
        {
            base.Enter();
            Duration = PlayerCharacter.DistanceFromHome;
        }

        private void ReturnToVehicle()
        {
            Assert.IsTrue(PlayerCharacter.DistanceFromHome == 0);
            if (PlayerCharacter.DistanceFromHome == 1)
            {
                PlayerCharacter.Return();
            }
            PlayerCharacter.Inventory().MoveAllResources(WorldState.HomeInventory());
        }
    }
}