﻿using System.Collections;
using DG.Tweening;
using Game.Combat.Player;
using Game.Combat.Ui;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Combat.Generation
{
    public sealed class ContainerBehaviour : MonoBehaviour
    {
        private const float MinDistanceToReveal = 0.5f;
        private float _currentFlashIntensity;
        public ContainerController ContainerController;
        private bool _revealed;
        private const float MaxRevealTime = 1f;
        private SpriteRenderer _glowSprite, _iconSprite;
        private InsectBehaviour _insectBehaviour;

        public void Awake()
        {
            _glowSprite = Helper.FindChildWithName<SpriteRenderer>(gameObject, "Glow");
            _glowSprite.color = UiAppearanceController.InvisibleColour;
            _iconSprite = Helper.FindChildWithName<SpriteRenderer>(gameObject, "Icon");
            _iconSprite.color = UiAppearanceController.InvisibleColour;
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

        public IEnumerator Fade()
        {
            _fading = true;
            _insectBehaviour.Fade();
            PlayerUi.FadeTextOut();
            _glowSprite.DOColor(UiAppearanceController.InvisibleColour, MaxRevealTime);
            _iconSprite.DOColor(UiAppearanceController.FadedColour, MaxRevealTime);
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
        }

        private float _lastDistance = -1;
        private bool _fading;

        public void Update()
        {
            if (_fading) return;
            float distanceToPlayer = Vector2.Distance(transform.position, PlayerCombat.Instance.transform.position);
            if (distanceToPlayer <= MinDistanceToReveal && _lastDistance > MinDistanceToReveal)
            {
                Reveal();
                PlayerUi.SetEventText("Take " + ContainerController.GetContents() + " [T]");
            }
            else if (distanceToPlayer > MinDistanceToReveal && _lastDistance <= MinDistanceToReveal && _revealed)
            {
                PlayerUi.FadeTextOut();
            }

            _lastDistance = distanceToPlayer;
        }

        public bool Revealed()
        {
            return _revealed;
        }
    }
}