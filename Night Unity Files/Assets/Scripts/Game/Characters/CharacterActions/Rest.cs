using Game.Global;

namespace Game.Characters.CharacterActions
{
	public class Rest : BaseCharacterAction
	{
		private int _timePassed;

		public Rest(Player playerCharacter) : base(nameof(Rest), playerCharacter)
		{
			DisplayName = "Resting";
			MinuteCallback = () =>
			{
				--_timePassed;
				if (_timePassed != 0) return;
				playerCharacter.Rest();
				ResetTimePassed();
			};
		}

		public override void Enter()
		{
			base.Enter();
			ResetTimePassed();
		}

		private void ResetTimePassed()
		{
			_timePassed = WorldState.MinutesPerHour / 4;
		}

		protected override void OnClick()
		{
		}

		public override float GetNormalisedProgress() => 0;
	}
}