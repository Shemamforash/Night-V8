using Characters;

namespace Game.Characters.CharacterActions
{
    public class Sleep : BaseCharacterAction
    {
        public Sleep(Character character) : base("Sleep", character)
        {
            SetUpdateCallback(() =>
            {
                Character.Endurance.Val += 3;
                if (Character.Endurance.Val == Character.Endurance.Max)
                {
                    Exit();
                }
            });
        }
        
        public override string GetCostAsString()
        {
            return TimeRemainingAsHours() + " hrs & +" + 3 * TimeRemainingAsHours() + " end ";
        }
    }
}