using Game.Combat.Generation;
using UnityEngine;
using UnityEngine.Assertions;

namespace Game.Combat.Enemies.Humans
{
    public class Medic : ArmedBehaviour
    {
        private const int HealAmount = 10;
        private const int HealTicks = 5;
        private const float HealDuration = 2f;
        private const float HealInterval = HealDuration / HealTicks;
        private static GameObject _healPrefab;
        private EnemyBehaviour _healTarget;
        private Cell _targetLastCell;

        public override void Initialise(Enemy enemy)
        {
            base.Initialise(enemy);
            if (_healPrefab == null) _healPrefab = Resources.Load<GameObject>("Prefabs/Combat/Heal Indicator");
//            MinimumFindCoverDistance = 20f;
        }

        public override void Update()
        {
            base.Update();
            CheckHealTarget();
        }

        private void CheckHealTarget()
        {
            if (_healTarget == null) return;
            if (_healTarget.IsDead)
            {
                _healTarget = null;
                _targetLastCell = null;
                ChooseNextAction();
                return;
            }

            if (Vector2.Distance(_healTarget.CurrentCell().Position, CurrentCell().Position) <= 0.25f)
            {
                Heal();
            }
            else if (_healTarget.CurrentCell() != _targetLastCell)
            {
                GetRouteToCell(_healTarget.CurrentCell());
                _targetLastCell = _healTarget.CurrentCell();
            }
        }

        public void RequestHeal(EnemyBehaviour healTarget)
        {
            Assert.IsNull(_healTarget);
            _healTarget = healTarget;
            _targetLastCell = healTarget.CurrentCell();
            GetRouteToCell(_healTarget.CurrentCell());
            SetActionText("Healing " + healTarget.Enemy.Template.EnemyType);
        }

        private void Heal()
        {
            float age = 0f;
            int currentTick = 1;
            SetActionText("Healing " + _healTarget.Enemy.Name);
            CurrentAction = () =>
            {
                age += Time.deltaTime;
                if (_healTarget == null || currentTick == HealTicks)
                {
                    _healTarget?.ChooseNextAction();
                    ChooseNextAction();
                    return;
                }

                if (age < currentTick * HealInterval) return;
                ++currentTick;
                _healTarget.HealthController.Heal(HealAmount);
                GameObject healObject = Instantiate(_healPrefab);
                healObject.transform.position = _healTarget.transform.position;
                healObject.transform.localScale = Vector3.one;
            };
        }

        public bool HasTarget()
        {
            return _healTarget != null;
        }
    }
}