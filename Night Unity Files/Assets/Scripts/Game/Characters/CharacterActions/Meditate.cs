using SamsHelper.BaseGameFunctionality.Basic;

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
            if (PlayerCharacter.Attributes.Get(AttributeType.Willpower).ReachedMax()) return;
            if (Duration == 0) SetDuration();
            Enter();
        }
    }
}