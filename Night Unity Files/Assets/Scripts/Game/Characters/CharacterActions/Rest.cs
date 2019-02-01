﻿namespace Game.Characters.CharacterActions
{
    public class Rest : BaseCharacterAction
    {
        public Rest(Player playerCharacter) : base(nameof(Rest), playerCharacter)
        {
            DisplayName = "Resting";
        }

        protected override void OnClick()
        {
        }

        public override float GetRemainingDuration()
        {
            return 0;
        }
    }
}