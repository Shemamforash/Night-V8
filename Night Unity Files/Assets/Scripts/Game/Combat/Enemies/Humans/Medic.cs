using UnityEngine;

namespace Game.Combat.Enemies.Humans
{
    public class Medic : EnemyBehaviour
    {
        private const int HealAmount = 10;
        private const int HealTicks = 5;
        private const float HealDuration = 2f;
        private const float HealInterval = HealDuration / HealTicks;
        private static GameObject _healPrefab;
        private EnemyBehaviour _healTarget;

        public override void Initialise(Enemy enemy)
        {
            base.Initialise(enemy);
            if (_healPrefab == null) _healPrefab = Resources.Load<GameObject>("Prefabs/Combat/Heal Indicator");
//            MinimumFindCoverDistance = 20f;
        }

        public void RequestHeal(EnemyBehaviour healTarget)
        {
            _healTarget = healTarget;
            MoveToCharacter(healTarget, Heal);
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