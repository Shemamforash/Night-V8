using Game.Combat.Generation;
using UnityEngine;
using UnityEngine.Assertions;

namespace Game.Combat.Enemies.Humans
{
    public class Medic : ArmedBehaviour
    {
        private static GameObject _healPrefab;
        private EnemyBehaviour _healTarget;

        public override void Initialise(Enemy enemy)
        {
            base.Initialise(enemy);
            if (_healPrefab == null) _healPrefab = Resources.Load<GameObject>("Prefabs/Combat/Visuals/Heal Indicator");
//            MinimumFindCoverDistance = 20f;
        }

        public override void MyUpdate()
        {
            base.MyUpdate();
            CheckHealTarget();
        }

        private void CheckHealTarget()
        {
            if (_healTarget == null) return;
            if (!_healTarget.IsDead()) return;
            _healTarget = null;
            TryFire();
        }

        protected override void ReachTarget()
        {
            Heal();
        }

        public void RequestHeal(EnemyBehaviour healTarget)
        {
            Assert.IsNull(_healTarget);
            _healTarget = healTarget;
            SetTarget(_healTarget);
            //todo FollowTarget();
        }

        private void Heal()
        {
            SkillAnimationController.Create("Medic", 1.5f, () =>
            {
                CombatManager.GetEnemiesInRange(transform.position, 2f).ForEach(e =>
                {
                    EnemyBehaviour enemy = e as EnemyBehaviour;
                    if (enemy == null) return;
                    int healAmount = (int) (enemy.HealthController.GetMaxHealth() * 0.2f);
                    enemy.HealthController.Heal(healAmount);
                    GameObject healObject = Instantiate(_healPrefab);
                    healObject.transform.position = _healTarget.transform.position;
                    healObject.transform.localScale = Vector3.one;
                });
                TryFire();
            });
        }

        public bool HasTarget()
        {
            return _healTarget != null;
        }
    }
}