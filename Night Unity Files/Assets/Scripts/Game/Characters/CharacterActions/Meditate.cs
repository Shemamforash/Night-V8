using SamsHelper.BaseGameFunctionality.Basic;

namespace Game.Characters.CharacterActions
{
    public class Meditate : BaseCharacterAction
    {
        public Meditate(Player playerCharacter) : base(nameof(Meditate), playerCharacter)
        {
            DisplayName = "Meditating";
            ShowTime = false;
            IsVisible = false;
            HourCallback = playerCharacter.Meditate;
        }

        protected override void OnClick()
        {
            if (PlayerCharacter.Attributes.Get(AttributeType.Willpower).ReachedMax()) return;
            Enter();
        }
    }
}