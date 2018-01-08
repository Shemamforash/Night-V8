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
            RegionManager.EnterManager(GetCharacter());
        }

        public void TravelToAndEnter(Region region)
        {
            SetDuration(region.Distance);
            AddOnExit(() => region.Enter(PlayerCharacter));
            Start();
        }

        public void TravelTo(Region region)
        {
            SetDuration(region.Distance);
            AddOnExit(() => region.Discover(PlayerCharacter));
            Start();
        }
    }
}