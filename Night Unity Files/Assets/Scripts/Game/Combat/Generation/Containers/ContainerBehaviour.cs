using System.Collections;
using DG.Tweening;
using Game.Combat.Player;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;

namespace Game.Combat.Generation
{
    public sealed class ContainerBehaviour : MonoBehaviour
    {
        private const float MinDistanceToReveal = 0.5f;
        private float _currentFlashIntensity;
        public ContainerController ContainerController;
        private bool _revealed;
        private const float MaxRevealTime = 1f;
        private ColourPulse _iconColour, _ringColour;
        private SpriteRenderer _glowSprite;

        public void Awake()
        {
            _glowSprite = Helper.FindChildWithName<SpriteRenderer>(gameObject, "Glow");
            _glowSprite.color = UiAppearanceController.InvisibleColour;
            _iconColour = Helper.FindChildWithName<ColourPulse>(gameObject, "Icon");
            _ringColour = Helper.FindChildWithName<ColourPulse>(gameObject, "Ring");
            _iconColour.SetAlphaMultiplier(0);
            _ringColour.SetAlphaMultiplier(0);
            StartCoroutine(TryReveal());
        }

        public void SetContainerController(ContainerController containerController)
        {
            ContainerController.Containers.Add(this);
            ContainerController = containerController;
        }

        public void Pulse()
        {
            _glowSprite.color = new Color(1, 1, 1, 0.3f);
            _glowSprite.DOFade(0f, 2f);
        }

        private void OnDestroy()
        {
            ContainerController.Containers.Remove(this);
        }

        private IEnumerator Reveal()
        {
            _revealed = true;
            float timePassed = 0f;
            _iconColour.enabled = true;
            _ringColour.enabled = true;
            while (timePassed < MaxRevealTime)
            {
                timePassed += Time.deltaTime;
                float alpha = timePassed / MaxRevealTime;
                if (alpha > 1) alpha = 1;
                _iconColour.SetAlphaMultiplier(alpha);
                _ringColour.SetAlphaMultiplier(alpha);
                yield return null;
            }

            _iconColour.SetAlphaMultiplier(1);
            _ringColour.SetAlphaMultiplier(1);
        }

        private IEnumerator TryReveal()
        {
            while (true)
            {
                float distanceToPlayer = Vector2.Distance(transform.position, PlayerCombat.Instance.transform.position);
                if (distanceToPlayer <= MinDistanceToReveal)
                {
                    yield return StartCoroutine(Reveal());
                    break;
                }

                yield return null;
            }
        }

        public bool Revealed()
        {
            return _revealed;
        }
    }
}