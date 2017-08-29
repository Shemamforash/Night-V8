using Characters;
using Game.World;

namespace Game.Characters.CharacterActions
{
    public class JourneyToLocation : BaseCharacterAction
    {
        private EnvironmentRegion _targetRegion;
        
        public JourneyToLocation(Character character) : base("Journey To Location", character)
        {
            IsVisible = false;
        }

        public void SetTargetRegion(EnvironmentRegion targetRegion)
        {
            _targetRegion = targetRegion;
            IncreaseDuration(_targetRegion.Distance());
            Start();
        }

        public override void Exit()
        {
            //TODO show resources
            Ready = false;
            ParentMachine.NavigateToState("Journey From Location");
            BaseCharacterAction journeyFrom = (BaseCharacterAction) ParentMachine.GetCurrentState();
            journeyFrom.IncreaseDuration(_targetRegion.Distance());
            journeyFrom.Start();
        }
    }
}