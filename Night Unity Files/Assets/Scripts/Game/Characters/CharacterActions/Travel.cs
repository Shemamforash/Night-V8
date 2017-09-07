using Characters;
using Game.Combat;
using Game.World.Region;

namespace Game.Characters.CharacterActions
{
    public class Travel : BaseCharacterAction
    {
        public Travel(Character character) : base("Travel", character)
        {
            IsVisible = false;
            HourCallback = GetCharacter().Travel;
        }

        public void SetTargetRegion(Region targetRegion)
        {
            GetCharacter().CurrentRegion = targetRegion;
            IncreaseDuration(GetCharacter().CurrentRegion.Distance());
            Start();
        }

        public Region GetTargetRegion()
        {
            return GetCharacter().CurrentRegion;
        }

        public override void Exit()
        {
            CombatScenario scenario = GetCharacter().CurrentRegion.GetCombatScenario();
            if (scenario != null)
            {
                ParentMachine.NavigateToState("Combat");
            }
            else
            {
                ParentMachine.NavigateToState("Collect Resources");
            }
            base.Exit(false);
        }
    }
}