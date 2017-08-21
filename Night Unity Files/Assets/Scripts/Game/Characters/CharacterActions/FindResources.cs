using Characters;
using Game.World;
using UnityEngine;

namespace Game.Characters.CharacterActions
{
    public class FindResources : BaseCharacterAction
    {
        private readonly int _enduranceCost;

        public FindResources(Character character) : base("Find Resources", character)
        {
            _enduranceCost = (int) Character.Weight;
            SetUpdateCallback(() => Character.Endurance.Val -= _enduranceCost);
        }

        public override bool IncreaseDuration()
        {
            if ((TimeRemainingAsHours() + 1) * _enduranceCost > Character.Endurance)
            {
                return false;
            }
            return base.IncreaseDuration();
        }

        public override string GetCostAsString()
        {
            return TimeRemainingAsHours() + " hrs & -" + _enduranceCost * TimeRemainingAsHours() + " end ";
        }

        public override void Exit()
        {
            Home.IncrementResource(ResourceType.Water, 1);
            Home.IncrementResource(ResourceType.Food, 1);
            base.Exit();
        }
    }
}