using Game.Global;
using SamsHelper.BaseGameFunctionality.Basic;

namespace Game.Characters.CharacterActions
{
    public class Meditate : BaseCharacterAction
    {
        public Meditate(Player playerCharacter) : base(nameof(Meditate), playerCharacter)
        {
            DisplayName = "Meditating\n+Perception +Willpower";
            HourCallback = playerCharacter.Meditate;
            MinuteCallback = () =>
            {
                if (Duration == 0) SetDuration(WorldState.MinutesPerHour);
                --Duration;
            };
        }

        protected override void OnClick()
        {
            if (PlayerCharacter.Attributes.Get(AttributeType.Willpower).ReachedMax()) return;
            if (Duration == 0) SetDuration(WorldState.MinutesPerHour);
            Enter();
        }
    }
}