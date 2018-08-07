using Game.Global;
using SamsHelper.BaseGameFunctionality.Basic;

namespace Game.Characters.CharacterActions
{
    public class Sleep : BaseCharacterAction
    {
        public Sleep(Player playerCharacter) : base(nameof(Sleep), playerCharacter)
        {
            DisplayName = "Sleeping\n+Endurance +Strength";
            HourCallback = playerCharacter.Sleep;
            MinuteCallback = () =>
            {
                if (Duration == 0) SetDuration(WorldState.MinutesPerHour);
                --Duration;
            };
        }

        protected override void OnClick()
        {
            if (PlayerCharacter.Attributes.Get(AttributeType.Strength).ReachedMax()) return;
            if (Duration == 0) SetDuration(WorldState.MinutesPerHour);
            Enter();
        }
    }
}