using UnityEngine.Assertions;

namespace Game.Characters.CharacterActions
{
    public class Idle : BaseCharacterAction
    {
        public Idle(Player playerCharacter) : base("Idle", playerCharacter)
        {
            IsVisible = false;
            HourCallback = () => GetCharacter().Rest(1);
        }

        public override void Enter()
        {
            Assert.AreEqual(GetCharacter().DistanceFromHome, 0);
            GetCharacter().CharacterView.SetActionListActive(true);
        }

        public override void Exit()
        {
            GetCharacter().CharacterView.SetActionListActive(false);
        }
        
        public override string GetCostAsString()
        {
            return "";
        }
    }
}