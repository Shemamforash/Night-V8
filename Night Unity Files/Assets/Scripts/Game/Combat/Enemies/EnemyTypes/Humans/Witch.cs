using System;
using System.Collections.Generic;
using Game.Combat.Enemies.EnemyTypes.Misc;
using Game.Gear.Weapons;
using UnityEngine;
using static Game.Combat.CombatManager;
using Random = UnityEngine.Random;

namespace Game.Combat.Enemies.EnemyTypes.Humans
{
    public class Witch : Enemy
    {
        private int _damageTaken;
        private float _timeSinceThrown, _targetTime;
        private bool _throwing;

        public Witch(float position) : base(nameof(Witch), 5, 5, position)
        {
            Weapon weapon = WeaponGenerator.GenerateWeapon(new List<WeaponType> {WeaponType.Pistol, WeaponType.SMG});
            Equip(weapon);
            ArmourLevel.SetCurrentValue(4);
            MinimumFindCoverDistance = 5f;
        }

        private Action ThrowGrenade()
        {
            float throwDuration = 2f;
            EnemyView.SetActionText("Throwing Grenade");
            return () =>
            {
                throwDuration -= Time.deltaTime;
                if (throwDuration > 0) return;
                float currentPosition = Position.CurrentValue();
                float targetPosition = Player.Position.CurrentValue();
                switch (Random.Range(0, 3))
                {
                    case 0:
                        UIGrenadeController.AddGrenade(new Grenade(currentPosition, targetPosition));
                        break;
                    case 1:
                        UIGrenadeController.AddGrenade(new SplinterGrenade(currentPosition, targetPosition));
                        break;
                    case 2:
                        UIGrenadeController.AddGrenade(new IncendiaryGrenade(currentPosition, targetPosition));
                        break;
                }

                _targetTime = Random.Range(10, 15);
                _throwing = false;
                ChooseNextAction();
            };
        }

        public override void Update()
        {
            base.Update();
            if (!InCombat() || _throwing) return;
            _timeSinceThrown += Time.deltaTime;
            if (!(_timeSinceThrown >= _targetTime)) return;
            CurrentAction = ThrowGrenade();
            _throwing = true;
        }
    }
}