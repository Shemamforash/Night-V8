using System.Collections;
using DG.Tweening;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;

namespace Game.Combat.Enemies.Nightmares.EnemyAttackBehaviours
{
    public class Shield : MonoBehaviour
    {
        private ParticleSystem _particles;
        private SpriteRenderer _sprite;
        private int duration = 0;

        public void Awake()
        {
            _sprite = gameObject.FindChildWithName<SpriteRenderer>("Hit");
            _particles = gameObject.FindChildWithName<ParticleSystem>("Particles");
            _sprite.color = UiAppearanceController.InvisibleColour;
            SetParticleColour(0f);
        }

        private void SetParticleColour(float val)
        {
            if (val == 0f) _particles.Stop();
            else if(!_particles.isPlaying) _particles.Play();
        }

        public void Activate(float duration)
        {
            StartCoroutine(ActivateShield(duration));
        }

        private IEnumerator ActivateShield(float f)
        {
            _sprite.color = Color.white;
            SetParticleColour(0.1f);
            while (f > 0f)
            {
                f -= Time.deltaTime;
                yield return null;
            }

            SetParticleColour(0f);
            _sprite.DOColor(UiAppearanceController.InvisibleColour, 0.5f);
        }
    }
}