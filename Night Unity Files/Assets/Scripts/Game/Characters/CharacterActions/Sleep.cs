using SamsHelper.BaseGameFunctionality.Basic;

namespace Game.Characters.CharacterActions
{
    public class Sleep : BaseCharacterAction
    {
        public Sleep(Player playerCharacter) : base(nameof(Sleep), playerCharacter)
        {
            DisplayName = "Sleeping\n+End +Str";
            MinuteCallback = () =>
            {
                --Duration;
                if (Duration != 0) return;
                playerCharacter.Sleep();
                SetDuration();
            };
        }

        protected override void OnClick()
        {
            if (PlayerCharacter.Attributes.Get(AttributeType.Strength).ReachedMax()) return;
            if (Duration == 0) SetDuration();
            Enter();
        }
    }
}