using System.Collections;
using DG.Tweening;
using Game.Combat.Misc;
using Game.Combat.Player;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;

namespace Game.Combat.Generation
{
    public sealed class ContainerBehaviour : MonoBehaviour, ICombatEvent
    {
        private const float MinDistanceToReveal = 0.5f;
        private float _currentFlashIntensity;
        public ContainerController ContainerController;
        private bool _revealed;
        private const float MaxRevealTime = 1f;
        private SpriteRenderer _glowSprite, _iconSprite, _ringSprite;
        private InsectBehaviour _insectBehaviour;
        private bool _fading;

        public void Awake()
        {
            _glowSprite = gameObject.FindChildWithName<SpriteRenderer>("Glow");
            _glowSprite.color = UiAppearanceController.InvisibleColour;
            _iconSprite = gameObject.FindChildWithName<SpriteRenderer>("Icon");
            _iconSprite.color = UiAppearanceController.InvisibleColour;
            _ringSprite = gameObject.FindChildWithName<SpriteRenderer>("Ring");
            _ringSprite.color = UiAppearanceController.InvisibleColour;
        }

        public void SetContainerController(ContainerController containerController)
        {
            ContainerController.Containers.Add(this);
            ContainerController = containerController;
            _iconSprite.sprite = Resources.Load<Sprite>("Images/Container Symbols/" + containerController.GetImageLocation());
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

        public void SetInsect(InsectBehaviour insectBehaviour)
        {
            _insectBehaviour = insectBehaviour;
        }

        private IEnumerator Fade()
        {
            _fading = true;
            if (_insectBehaviour != null) _insectBehaviour.Fade();
            _glowSprite.DOColor(UiAppearanceController.InvisibleColour, MaxRevealTime);
            _iconSprite.DOColor(UiAppearanceController.InvisibleColour, MaxRevealTime);
            _ringSprite.DOColor(UiAppearanceController.InvisibleColour, MaxRevealTime);
            Tween t = _iconSprite.DOColor(UiAppearanceController.InvisibleColour, MaxRevealTime);
            yield return t.WaitForCompletion();
            Destroy(this);
        }

        public void Reveal()
        {
            if (_revealed) return;
            CombatManager.IncreaseItemsFound();
            _revealed = true;
            _iconSprite.DOColor(new Color(1, 1, 1, 0.6f), MaxRevealTime);
            _ringSprite.DOColor(new Color(1, 1, 1, 0.6f), MaxRevealTime);
        }

        public bool Revealed()
        {
            return _revealed;
        }

        public float InRange()
        {
            if (_fading) return -1;
            float distanceToPlayer = Vector2.Distance(transform.position, PlayerCombat.Instance.transform.position);
            if (distanceToPlayer > MinDistanceToReveal) return -1;
            Reveal();
            return distanceToPlayer;
        }

        public string GetEventText()
        {
            return "Take " + ContainerController.GetContents() + " [T]";
        }

        public void Activate()
        {
            ContainerController.Take();
            StartCoroutine(Fade());
        }
    }
}