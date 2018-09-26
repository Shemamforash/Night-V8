using Game.Combat.Player;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;

namespace Facilitating.UIControllers
{
    public class HeartBeatController : MonoBehaviour
    {
        private float _currentTime;
        private readonly float _heartBeatLongGap = 1f;

        private ParticleSystem _heartBeatParticles;
        private readonly float _heartBeatShortGap = 0.25f;
        private bool _timingLongBeat;

        public void Awake()
        {
            _heartBeatParticles = GetComponent<ParticleSystem>();
        }

        public void Start()
        {
            SetTrailAlpha(0f);
        }

        private void TryPlayParticles(float health)
        {
            if (health <= 0.4f)
            {
                if (!_heartBeatParticles.isPlaying) _heartBeatParticles.Play();
            }
            else if (_heartBeatParticles.isPlaying)
            {
                _heartBeatParticles.Stop();
            }
        }

        public void Update()
        {
            float health = PlayerCombat.Instance.HealthController.GetNormalisedHealthValue();
            TryPlayParticles(health);
            if (health >= 0.4f) return;
            float maxAlpha = 1f - health / 0.4f;
            float minAlpha = Mathf.Clamp(maxAlpha - 0.6f, 0f, 1f);
            float currentAlpha;
            _currentTime += Time.deltaTime;
            if (_timingLongBeat)
            {
                if (_currentTime >= _heartBeatLongGap)
                {
                    _timingLongBeat = false;
                    _currentTime -= _heartBeatLongGap;
                }

                currentAlpha = 1f - _currentTime / _heartBeatLongGap;
            }
            else
            {
                if (_currentTime >= _heartBeatShortGap)
                {
                    _timingLongBeat = true;
                    _currentTime -= _heartBeatShortGap;
                }

                currentAlpha = 1f - _currentTime / _heartBeatShortGap;
            }

            if (currentAlpha < 0) currentAlpha = 0f;
            currentAlpha *= maxAlpha - minAlpha;
            currentAlpha += minAlpha;
            SetTrailAlpha(currentAlpha);
        }

        private void SetTrailAlpha(float alpha)
        {
            ParticleSystem.TrailModule trails = _heartBeatParticles.trails;
            trails.colorOverTrail = new ParticleSystem.MinMaxGradient(new Color(1, 1, 1, alpha), UiAppearanceController.InvisibleColour);
        }
    }
}