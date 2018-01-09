using Game.World.Region;

namespace Game.Characters.CharacterActions
{
    public class Travel : BaseCharacterAction
    {
        public Travel(Player playerCharacter) : base("Travel", playerCharacter)
        {
            HourCallback = GetCharacter().Travel;
        }
        
        public override void Enter()
        {
            base.Enter();
            if(PlayerCharacter.DistanceFromHome == 0) RegionManager.EnterManager(GetCharacter());
        }

        public void TravelTo(Region region)
        {
            Enter();
            SetDuration(region.Distance - PlayerCharacter.DistanceFromHome);
            AddOnExit(() => region.Discover(PlayerCharacter));
            Start();
        }
    }
}