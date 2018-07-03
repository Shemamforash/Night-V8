using Facilitating;

namespace Game.Characters.CharacterActions
{
    public class LightFire : BaseCharacterAction
    {
        public LightFire(Player playerCharacter) : base("Tend Fire", playerCharacter)
        {
            HourCallback = () =>
            {
                --Duration;
                if (Duration != 0) return;
                Campfire.Tend();
                Exit();
            };
        }

        public override void Enter()
        {
            base.Enter();
            Duration = 1;
        }
    }
}