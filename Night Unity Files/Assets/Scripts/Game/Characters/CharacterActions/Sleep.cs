using SamsHelper.BaseGameFunctionality.Basic;

namespace Game.Characters.CharacterActions
{
    public class Sleep : BaseCharacterAction
    {
        public Sleep(Player playerCharacter) : base(nameof(Sleep), playerCharacter)
        {
            DisplayName = "Sleeping\n+Endurance +Strength";
            ShowTime = false;
            IsVisible = false;
            HourCallback = playerCharacter.Sleep;
        }

        protected override void OnClick()
        {
            if (PlayerCharacter.Attributes.Get(AttributeType.Strength).ReachedMax()) return;
            Enter();
        }
    }
}