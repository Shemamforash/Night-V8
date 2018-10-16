using System.Collections;
using DG.Tweening;
using Facilitating.UIControllers;
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
        private SpriteRenderer _iconSprite, _ringSprite;
        private InsectBehaviour _insectBehaviour;
        private bool _fading, _showRing;

        public void Awake()
        {
            _iconSprite = gameObject.FindChildWithName<SpriteRenderer>("Icon");
            _iconSprite.color = UiAppearanceController.InvisibleColour;
            _ringSprite = gameObject.FindChildWithName<SpriteRenderer>("Ring");
            _ringSprite.color = UiAppearanceController.InvisibleColour;
            transform.rotation = Quaternion.Euler(0, 0, Random.Range(0, 360));
        }

        public void SetContainerController(ContainerController containerController)
        {
            ContainerController.Containers.Add(this);
            ContainerController = containerController;
            _iconSprite.sprite = Resources.Load<Sprite>("Images/Container Symbols/" + containerController.GetImageLocation());
            _showRing = containerController is Loot;
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
            _iconSprite.DOColor(UiAppearanceController.InvisibleColour, MaxRevealTime);
            _ringSprite.DOColor(UiAppearanceController.InvisibleColour, MaxRevealTime);
            Tween t = _iconSprite.DOColor(UiAppearanceController.InvisibleColour, MaxRevealTime);
            GetComponent<CompassItem>().Die();
            yield return t.WaitForCompletion();
            Destroy(this);
        }

        public void Reveal()
        {
            if (_revealed) return;
            _revealed = true;
            _iconSprite.DOColor(new Color(1, 1, 1, 0.6f), MaxRevealTime);
            if (!_showRing) return;
            _ringSprite.DOColor(new Color(1, 1, 1, 0.6f), MaxRevealTime);
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