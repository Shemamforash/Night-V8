using Facilitating;
using Game.Global;

namespace Game.Characters.CharacterActions
{
    public class LightFire : BaseCharacterAction
    {
        public LightFire(Player playerCharacter) : base("Tend Fire", playerCharacter)
        {
            MinuteCallback = () =>
            {
                --Duration;
                Campfire.Tend();
                if (Duration != 0) return;
                Exit();
            };
        }

        public override void Enter()
        {
            base.Enter();
            SetDuration(WorldState.MinutesPerHour);
        }
    }
}