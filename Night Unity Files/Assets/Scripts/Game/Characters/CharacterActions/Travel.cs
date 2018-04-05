using Game.World.Region;
using NUnit.Framework;

namespace Game.Characters.CharacterActions
{
    public class Travel : BaseCharacterAction
    {
        //todo fix me
        
        private Region _targetRegion;

        public Travel(Player.Player playerCharacter) : base("Travel", playerCharacter)
        {
            HourCallback = () =>
            {
                --Duration;
                PlayerCharacter.Travel();
                if (Duration != 0) return;
//                _targetRegion.Discover(playerCharacter);
                Exit();
            };
        }

        protected override void OnClick()
        {
            Assert.IsTrue(PlayerCharacter.DistanceFromHome == 0);
            UIMapController.Open();
        }

        public void TravelTo(Region region)
        {
//            Duration = region.Distance - PlayerCharacter.DistanceFromHome;
            _targetRegion = region;
            Enter();
        }
    }
}