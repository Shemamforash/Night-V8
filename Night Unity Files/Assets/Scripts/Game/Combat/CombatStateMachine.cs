using Game.Combat.CombatStates;
using SamsHelper.BaseGameFunctionality.StateMachines;

namespace Game.Combat
{
    public class CombatStateMachine : StateMachine
    {
        public void Awake()
        {
            AddState(new Approaching(this, true));
            AddState(new Aiming(this, true));
            AddState(new Cocking(this, true));
            AddState(new EnteringCover(this, true));
            AddState(new ExitingCover(this, true));
            AddState(new Firing(this, true));
            AddState(new Flanking(this, true));
            AddState(new Reloading(this, true));
            AddState(new Retreating(this, true));
        }
    }
}