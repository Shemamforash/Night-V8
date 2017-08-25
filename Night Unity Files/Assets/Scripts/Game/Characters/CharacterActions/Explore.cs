using Characters;
using Game.World;

namespace Game.Characters.CharacterActions
{
    public class Explore : BaseCharacterAction
    {
        public Explore(Character character) : base("Explore", character)
        {
        }

        public override void Enter()
        {
            WorldState.MenuNavigator.SwitchToMenu("Region Menu", true);
        }
    }
}