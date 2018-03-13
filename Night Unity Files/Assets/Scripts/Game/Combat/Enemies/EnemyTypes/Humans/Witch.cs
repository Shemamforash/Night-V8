﻿using System;
using System.Collections.Generic;
using Facilitating.UIControllers;
using Game.Characters;
using Game.Combat.Enemies.EnemyTypes.Misc;
using Game.Gear.Weapons;
using UnityEngine;
using static Game.Combat.CombatManager;
using Random = UnityEngine.Random;

namespace Game.Combat.Enemies.EnemyTypes.Humans
{
    public class Witch : DetailedEnemyCombat
    {
        private int _damageTaken;
        private float _targetTime;
        private bool _throwing;

        public override void SetPlayer(Character enemy)
        {
            base.SetPlayer(enemy);
//            MinimumFindCoverDistance = 5f;
        }

        private Action ThrowGrenade()
        {
            _throwing = true;
            float throwDuration = 2f;
            SetActionText("Throwing Grenade");
            return () =>
            {
                throwDuration -= Time.deltaTime;
                if (throwDuration > 0) return;
//                float currentPosition = Position.CurrentValue();
                //todo get player
//                float targetPosition = Player.Position.CurrentValue();
//                switch (Random.Range(0, 3))
//                {
//                    case 0:
//                        UIGrenadeController.AddGrenade(GrenadeType.Grenade, currentPosition, targetPosition);
//                        break;
//                    case 1:
//                        UIGrenadeController.AddGrenade(GrenadeType.Incendiary, currentPosition, targetPosition);
//                        break;
//                    case 2:
//                        UIGrenadeController.AddGrenade(GrenadeType.Splinter, currentPosition, targetPosition);
//                        break;
//                }

                _targetTime = Random.Range(10, 15);
                _throwing = false;
                ChooseNextAction();
            };
        }

        public override void Alert()
        {
            base.Alert();
//            if(!Alerted) _targetTime = Random.Range(10, 15);
        }

        public override void Update()
        {
            if(MeleeController.InMelee) return;
            base.Update();
            if (!InCombat() || _throwing) return;
            _targetTime -= Time.deltaTime;
            if (_targetTime > 0) return;
//            CurrentAction = ThrowGrenade();
        }
    }
}