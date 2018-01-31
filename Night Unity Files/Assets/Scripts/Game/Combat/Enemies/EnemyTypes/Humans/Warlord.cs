using Game.Combat.Enemies.EnemyTypes.Misc;
using Game.Gear.Weapons;
using UnityEngine;
using static UIEnemyController;

namespace Game.Combat.Enemies.EnemyTypes.Humans
{
    public class Warlord : Enemy
    {
        private float _reinforceCallTime;
        private float _reinforceDuration = 5f;

        public Warlord(float position) : base("Warlord", 10, 3, position)
        {
            Weapon weapon = WeaponGenerator.GenerateWeapon(WeaponType.LMG);
            Equip(weapon);
            ArmourLevel.SetCurrentValue(8);
            MinimumFindCoverDistance = 10;
            HealthController.AddOnTakeDamage(a =>
            {
                if (ReachedMaxEncounterSize()) return;
                float normalHealthBefore = (HealthController.GetCurrentHealth() + a) / HealthController.GetMaxHealth();
                float currentNormalHealth = HealthController.GetNormalisedHealthValue();
                if (normalHealthBefore > 0.25f && currentNormalHealth <= 0.25f
                    || normalHealthBefore > 0.5f && currentNormalHealth <= 0.5f
                    || normalHealthBefore > 0.75f && currentNormalHealth <= 0.75f)
                {
                    CurrentAction = SummonEnemies;
                    _reinforceCallTime = _reinforceDuration;
                }
            });
        }

        private void SummonEnemies()
        {
            EnemyView.SetActionText("Reinforcing");
            _reinforceCallTime -= Time.deltaTime;
            if (!(_reinforceCallTime <= 0)) return;
            QueueEnemyToAdd(new Sentinel(CombatManager.VisibilityRange + BasicEnemyView.FadeVisibilityDistance));
            ChooseNextAction();
            return;
//            switch (Random.Range(0, 4))
//            {
//                case 0:
//                    CombatManager.QueueEnemyToAdd(new Fighter());
//                    break;
//                case 1:
//                    CombatManager.QueueEnemyToAdd(new Sniper());
//                    break;
//                case 2:
//                    CombatManager.QueueEnemyToAdd(new Martyr());
//                    break;
//                case 3:
//                    CombatManager.QueueEnemyToAdd(new Medic());
//                    break;
//            }
        }
    }
}