using Facilitating;
using Game.Global;

namespace Game.Characters.CharacterActions
{
    public class LightFire : BaseCharacterAction
    {
        public LightFire(Player playerCharacter) : base("Tend Fire", playerCharacter)
        {
            DisplayName = "Lighting Fire";
            MinuteCallback = () =>
            {
                --Duration;
                Campfire.Tend();
                if (Duration != 0) return;
                playerCharacter.RestAction.Enter();
            };
        }

        public override void Enter()
        {
            SetDuration();
            base.Enter();
        }
    }
}