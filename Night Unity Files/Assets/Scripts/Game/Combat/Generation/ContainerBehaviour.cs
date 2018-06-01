using System.Collections;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Combat.Generation
{
    public class ContainerBehaviour : MonoBehaviour
    {
        private const int MinDistanceToReveal = 1;
        private float _currentFlashIntensity;
        public ContainerController ContainerController;
        private bool _revealed;
        private const float MaxRevealTime = 1f;
        private ColourPulse _iconColour, _ringColour, _glowColour;

        public virtual void Awake()
        {
            _iconColour = Helper.FindChildWithName<ColourPulse>(gameObject, "Icon");
            _ringColour = Helper.FindChildWithName<ColourPulse>(gameObject, "Ring");
            _glowColour = Helper.FindChildWithName<ColourPulse>(gameObject, "Glow");
            _iconColour.SetAlphaMultiplier(0);
            _ringColour.SetAlphaMultiplier(0);
            _glowColour.SetAlphaMultiplier(0);
        }

        public void SetContainerController(ContainerController containerController)
        {
            ContainerController.Containers.Add(this);
            ContainerController = containerController;
        }

        public void Update()
        {
            float distanceToPlayer = Vector2.Distance(transform.position, CombatManager.Player().transform.position);
            if (distanceToPlayer > 10f)
            {
                _glowColour.SetAlphaMultiplier(0);
                return;
            }
            _glowColour.SetAlphaMultiplier(1f - distanceToPlayer / 10f);
        }
        
        private void OnDestroy()
        {
            ContainerController.Containers.Remove(this);
        }

        private IEnumerator Reveal()
        {
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

        public void TryReveal()
        {
            if (_revealed) return;
            float distanceToPlayer = Vector2.Distance(transform.position, CombatManager.Player().transform.position);
            if (distanceToPlayer > MinDistanceToReveal) return;
            _revealed = true;
            StartCoroutine(Reveal());
        }

        public bool Revealed()
        {
            return _revealed;
        }
    }
}