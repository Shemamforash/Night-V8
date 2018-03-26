using System;
using System.Collections.Generic;
using Facilitating.UIControllers;
using Game.Characters;
using Game.Combat.CharacterUi;
using Game.Combat.Enemies.EnemyTypes.Misc;
using Game.Gear.Weapons;
using UnityEngine;
using static Game.Combat.CombatManager;
using Random = UnityEngine.Random;

namespace Game.Combat.Enemies.EnemyTypes.Humans
{
    public class Witch : EnemyBehaviour
    {
        private int _damageTaken;
        private bool _throwing;
        private float _cooldownTime;

        public override void Initialise(Enemy enemy, EnemyUi characterUi)
        {
            base.Initialise(enemy, characterUi);
        }

        private Action ThrowGrenade()
        {
            _throwing = true;
            float throwDuration = 1f;
            SetActionText("Throwing Grenade");
            return () =>
            {
                throwDuration -= Time.deltaTime;
                if (throwDuration > 0) return;
                Vector2 currentPosition = transform.position;
                //todo get player
                Vector2 targetPosition = GetTarget().transform.position;
                switch (Random.Range(0, 3))
                {
                    case 0:
                        Grenade.Create(currentPosition, targetPosition);
                        break;
                    case 1:
                        Grenade.Create(currentPosition, targetPosition);
                        break;
                    case 2:
                        Grenade.Create(currentPosition, targetPosition);
                        break;
                }

                _cooldownTime = Random.Range(10, 15);
                _throwing = false;
                ChooseNextAction();
            };
        }

        protected override void OnAlert()
        {
            base.OnAlert();
            _cooldownTime = Random.Range(10, 15);
        }

        public override void Update()
        {
            base.Update();
            if (_throwing || !Alerted) return;
            _cooldownTime -= Time.deltaTime;
            if (_cooldownTime > 0) return;
            if(CouldHitTarget)
            CurrentAction = ThrowGrenade();
        }
    }
}