using System.Collections.Generic;
using Game.Combat.Generation;
using Game.Combat.Misc;
using UnityEngine;

namespace Game.Combat.Enemies.Nightmares
{
    public class Nightmare : EnemyBehaviour
    {
        private bool drawnlife;
        private const float DrawLifeTimerMax = 5f;
        private const float BeamAttackTimerMax = 7f;
        private float _drawLifeTimer;
        private float _beamAttackTimer;
        private int _drawLifeCount;
        private bool _firing;
        private bool _drawingLife;

        public override void Update()
        {
            base.Update();
            UpdateDrawLife();
            UpdateBeamAttack();
        }

        private void UpdateBeamAttack()
        {
            Immobilised(true);
            _beamAttackTimer -= Time.deltaTime;
            if (_beamAttackTimer > 0f) return;
            BeamController.Create(transform.position, transform.right);
            BeamController.Create(transform.position, -transform.right);
            BeamController.Create(transform.position, transform.up);
            BeamController.Create(transform.position, -transform.up);
            _beamAttackTimer = BeamAttackTimerMax;
        }

        public void DecreaseDrawLifeCount(int health)
        {
            HealthController.Heal(health);
            --_drawLifeCount;
            if (_drawLifeCount > 0) return;
            Immobilised(false);
            _drawLifeTimer = DrawLifeTimerMax;
        }

        private void UpdateDrawLife()
        {
            if (_drawLifeTimer > 0f)
            {
                _drawLifeTimer -= Time.deltaTime;
                return;
            }

            if (_drawLifeCount > 0) return;
            List<CharacterCombat> charactersInRange = CombatManager.GetCharactersInRange(transform.position, 5f);
            int maxDraw = Random.Range(2, 5);
            foreach (CharacterCombat c in charactersInRange)
            {
                if (maxDraw == 0) return;
                Ghoul g = c as Ghoul;
                if (g == null) continue;
                --maxDraw;
                g.StartDrawLife(this);
                Immobilised(true);
                ++_drawLifeCount;
            }
        }
    }
}