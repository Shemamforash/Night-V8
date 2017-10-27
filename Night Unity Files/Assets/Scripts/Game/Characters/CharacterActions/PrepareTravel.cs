using Game.World;
using Game.World.Region;

namespace Game.Characters.CharacterActions
{
    public class PrepareTravel : BaseCharacterAction
    {
        public PrepareTravel(Player playerCharacter) : base("Prepare Travel", playerCharacter)
        {
        }

        public override void Enter()
        {
            RegionManager.EnterManager(GetCharacter());
        }
    }
}