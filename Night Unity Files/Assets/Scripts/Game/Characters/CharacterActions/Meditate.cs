using Game.Global;

namespace Game.Characters.CharacterActions
{
    public class Meditate : BaseCharacterAction
    {
        public Meditate(Player playerCharacter) : base(nameof(Meditate), playerCharacter)
        {
            DisplayName = "Meditating\n+Per +Wil";
            MinuteCallback = () =>
            {
                --Duration;
                if (Duration != 0) return;
                playerCharacter.Meditate();
                SetDuration();
            };
        }

        protected override void OnClick()
        {
            int maxMeditateTime = PlayerCharacter.GetMaxMeditateTime();
            if (maxMeditateTime == 0) return;
            SetDuration(maxMeditateTime * WorldState.MinutesPerHour / 2);
            Enter();
        }
    }
}