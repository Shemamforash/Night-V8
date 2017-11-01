using System.Collections.Generic;
using SamsHelper.BaseGameFunctionality.StateMachines;
using Random = UnityEngine.Random;

namespace Game.Combat.Enemies.EnemyBehaviours
{
    public abstract class EnemyBehaviour : State
    {
        protected readonly EnemyPlayerRelation Relation;
        protected float Duration;
        private readonly List<EnemyBehaviour> _onExitTransitions = new List<EnemyBehaviour>();
        private EnemyBehaviour _onDetectBehaviour; 
        public readonly bool IsPassive;

        protected EnemyBehaviour(string name, EnemyPlayerRelation relation, bool isPassive = false) : base(name, StateSubtype.EnemyBehaviour)
        {
            Relation = relation;
            IsPassive = isPassive;
        }

        public void SetOnDetectBehaviour(EnemyBehaviour behaviour)
        {
            _onDetectBehaviour = behaviour;
        }

        protected override void ReturnToDefault()
        {
            Relation.Enemy.CombatStates.ReturnToDefault();
        }
        
        protected void NavigateToCombatState(string stateName)
        {
            Relation.Enemy.CombatStates.NavigateToState(stateName);
        }

        protected override void NavigateToState(string stateName)
        {
            Relation.Enemy.BehaviourMachine.NavigateToState(stateName);
        }

        protected void SelectRandomTransition()
        {
            NavigateToState(_onExitTransitions[Random.Range(0, _onExitTransitions.Count)].Name);
        }
        
        public void AddExitTransition(EnemyBehaviour transition)
        {
            if (!_onExitTransitions.Contains(transition))
            {
                _onExitTransitions.Add(transition);
            }
        }

        protected void SetStatusText(string text)
        {
            Relation.Enemy.SetActionText(text);
        }

        public virtual void OnDetect()
        {
            NavigateToState(_onDetectBehaviour.Name);
        }
    }
}