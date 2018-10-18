using DG.Tweening;
using Game.Combat.Enemies.Misc;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Combat.Enemies.Humans
{
    public class Mountain : ArmedBehaviour
    {
        private bool _pushing;
        private float _forceCooldown;
        private const float MinDistanceToTarget = 1.5f;

        private void ResetCooldown()
        {
            _forceCooldown = Random.Range(5, 10);
            _pushing = false;
        }

        private void Push()
        {
            Sequence sequence = DOTween.Sequence();
            sequence.AppendCallback(() => SkillAnimationController.Create(transform, "Beam", 0.25f, () => { PushController.Create(transform.position, 0f, false, 360); }, 0.5f));
            sequence.AppendInterval(0.25f);
            sequence.AppendCallback(() => SkillAnimationController.Create(transform, "Beam", 0.25f, () => { PushController.Create(transform.position, 0f, false, 360); }, 0.5f));
            sequence.AppendInterval(0.5f);
            sequence.AppendCallback(() =>
            {
                for (int i = 0; i < 10; ++i)
                {
                    Vector2 position = AdvancedMaths.RandomVectorWithinRange(Vector2.zero, 1);
                    position.Normalize();
                    position = (Vector2) transform.position + position * Random.Range(1f, 2f);
                    Grenade.CreateBasic(transform.position, position, false);
                }

                ResetCooldown();
                TryFire();
            });
        }

        protected override void OnAlert()
        {
            base.OnAlert();
            ResetCooldown();
        }

        public override void MyUpdate()
        {
            base.MyUpdate();
            if (!Alerted || _pushing) return;
            _forceCooldown -= Time.deltaTime;
            if (_forceCooldown > 0) return;
            if (DistanceToTarget() > MinDistanceToTarget) return;
            CurrentAction = null;
            _pushing = true;
            //todo mountain skill image
            SkillAnimationController.Create(transform, "Sniper", 1f, Push);
        }
    }
}