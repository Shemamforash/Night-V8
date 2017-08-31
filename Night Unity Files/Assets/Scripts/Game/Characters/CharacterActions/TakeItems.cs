using Characters;
using Game.World;
using SamsHelper.ReactiveUI;
using SamsHelper.ReactiveUI.MenuSystem;

namespace Game.Characters.CharacterActions
{
    public class TakeItems : BaseCharacterAction
    {
        public TakeItems(Character character) : base("Take Items", character)
        {
            IsVisible = false;
        }

        public override void Enter()
        {
            MenuStateMachine.Instance.NavigateToState("Pick Up Menu");
        }
    }
}