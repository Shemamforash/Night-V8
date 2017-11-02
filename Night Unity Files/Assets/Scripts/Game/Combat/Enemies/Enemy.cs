using System.Linq;
using Game.Characters;
using Game.Combat.Enemies.EnemyBehaviours;
using Game.Gear.Weapons;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.StateMachines;
using SamsHelper.ReactiveUI;
using SamsHelper.ReactiveUI.InventoryUI;
using UnityEngine;

namespace Game.Combat.Enemies
{
    public class Enemy : Character
    {
        private MyValue _enemyHp;
        private MyValue _sightToCharacter;
        private MyValue _exposure;
        public CharacterAttribute VisionRange = new CharacterAttribute(AttributeType.Vision, 30f);
        public CharacterAttribute DetectionRange = new CharacterAttribute(AttributeType.Detection, 15f);
        protected readonly ValueTextLink<string> ActionTextLink = new ValueTextLink<string>();
        protected readonly ValueTextLink<string> AlertTextLink = new ValueTextLink<string>();

        private const float _movementSpeed = 1;
        private EnemyView _enemyView;
        public readonly StateMachine<EnemyBehaviour> BehaviourMachine = new StateMachine<EnemyBehaviour>();
        private const int EnemyBehaviourTick = 4;
        private int _timeSinceLastBehaviourUpdate = 0;
        private bool _alerted;
        private EnemyPlayerRelation _relation;
        private EnemyBehaviour _currentBehaviour;

        public bool IsAlerted()
        {
            return _alerted;
        }

        public void Alert()
        {
            if (_alerted) return;
            _alerted = true;
            BehaviourMachine.StatesAsList().ForEach(b => b.OnDetect());
        }

        public void SetActionText(string action)
        {
            ActionTextLink.Value(action);
        }

        private void SetAlertText(string alert)
        {
            AlertTextLink.Value(alert);
        }

        private void UpdateDetection()
        {
            if (_relation.Distance < DetectionRange || _alerted)
            {
                SetAlertText("Detected");
                Alert();
                return;
            }
            if (_relation.Distance < VisionRange)
            {
                SetAlertText("Alerted");
                return;
            }
            SetAlertText("Unaware");
        }

        protected void SetReciprocralBehaviour(EnemyBehaviour behaviour1, EnemyBehaviour behaviour2)
        {
            behaviour1.AddExitTransition(behaviour2);
            behaviour2.AddExitTransition(behaviour1);
        }

        public Enemy(string name, int enemyHp) : base(name)
        {
            _enemyHp = new MyValue(enemyHp, 0, enemyHp);
            _enemyHp.OnMin(Kill);
            Equip(WeaponGenerator.GenerateWeapon());
        }

        public EnemyBehaviour GetBehaviour(EnemyBehaviour behaviour)
        {
            return BehaviourMachine.StatesAsList().FirstOrDefault(b => b.GetType() == behaviour.GetType());
        }

        public virtual void InitialiseBehaviour(EnemyPlayerRelation relation)
        {
            _relation = relation;
        }

        public EnemyView EnemyView()
        {
            return _enemyView;
        }

        public override void TakeDamage(int amount)
        {
            Alert();
            _enemyHp.SetCurrentValue(_enemyHp.GetCurrentValue() - amount);
        }

        public override void Kill()
        {
            CombatManager.Flee(this);
        }

        public override ViewParent CreateUi(Transform parent)
        {
            _enemyView = new EnemyView(this, parent);
            _enemyHp.AddOnValueChange(f =>
            {
                float normalisedHealth = _enemyHp.GetCurrentValue() / _enemyHp.Max;
                _enemyView.SetHealth(normalisedHealth);
                _enemyView.StrengthText.text = Helper.Round(f.GetCurrentValue(), 0).ToString();
            });
            ActionTextLink.AddTextObject(_enemyView.ActionText);
            AlertTextLink.AddTextObject(_enemyView.AlertText);
            return _enemyView;
        }

        public void UpdateBehaviour()
        {
            UpdateDetection();
//            CombatController.Update();
            BehaviourMachine.Update();
        }

        public string EnemyType()
        {
            return "Default Enemy";
        }
    }
}