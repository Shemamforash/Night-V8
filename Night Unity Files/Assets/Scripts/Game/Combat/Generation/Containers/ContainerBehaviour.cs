using System.Collections;
using C5;
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
        private SpriteRenderer _iconSprite, _glowSprite, _selectSprite;
        private InsectBehaviour _insectBehaviour;
        private bool _fading;
        private float _targetAlpha;
        private float _currentAlpha;
        private ParticleSystem _puddleDrops;

        public void Awake()
        {
            _iconSprite = gameObject.FindChildWithName<SpriteRenderer>("Icon");
            _iconSprite.color = UiAppearanceController.InvisibleColour;
            _glowSprite = gameObject.FindChildWithName<SpriteRenderer>("Glow");
            _glowSprite.color = UiAppearanceController.InvisibleColour;
            _selectSprite = gameObject.FindChildWithName<SpriteRenderer>("Select");
            _selectSprite.color = UiAppearanceController.InvisibleColour;
        }

        public void SetContainerController(ContainerController containerController)
        {
            ContainerController.Containers.Add(this);
            ContainerController = containerController;
            _iconSprite.sprite = Resources.Load<Sprite>("Images/Container Symbols/" + containerController.GetImageLocation());
            if (!(containerController is WaterSource)) return;
            _puddleDrops = gameObject.FindChildWithName<ParticleSystem>("Drop Randomiser");
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
            _iconSprite.color = Color.white;
            _glowSprite.color = Color.white;
            _selectSprite.color = Color.white;
            if (_insectBehaviour != null) _insectBehaviour.Fade();
            _iconSprite.DOColor(UiAppearanceController.InvisibleColour, MaxRevealTime);
            _glowSprite.DOColor(UiAppearanceController.InvisibleColour, MaxRevealTime);
            _selectSprite.DOColor(UiAppearanceController.InvisibleColour, MaxRevealTime);
            Tween t = _iconSprite.DOColor(UiAppearanceController.InvisibleColour, MaxRevealTime);
            GetComponent<CompassItem>().Die();
            yield return t.WaitForCompletion();
            Destroy(this);
        }

        public void Update()
        {
            transform.rotation = Quaternion.Euler(0, 0, PlayerCombat.Instance.transform.rotation.z);
            if (!_revealed || _fading) return;
            if (_currentAlpha == _targetAlpha) return;
            if (_currentAlpha > _targetAlpha) _currentAlpha -= Time.deltaTime;
            else _currentAlpha += Time.deltaTime;
            _currentAlpha = Mathf.Clamp(_currentAlpha, 0f, 1f);
            _selectSprite.color = new Color(1, 1, 1, _currentAlpha);
        }

        public void Reveal()
        {
            if (_revealed) return;
            _revealed = true;
            _iconSprite.DOColor(new Color(1, 1, 1, 0.6f), MaxRevealTime);
        }

        public float InRange()
        {
            if (_fading) return -1;
            float distanceToPlayer = transform.position.Distance(PlayerCombat.Instance.transform.position);
            if (distanceToPlayer > MinDistanceToReveal)
            {
                _targetAlpha = 0f;
                return -1;
            }

            _targetAlpha = 1;
            Reveal();
            return distanceToPlayer;
        }

        public string GetEventText()
        {
            return "Take " + ContainerController.GetContents() + " [T]";
        }

        public void Activate()
        {
            if (_puddleDrops != null) _puddleDrops.Stop();
            ContainerController.Take();
            PlayerCombat.Instance.WeaponAudio.PlayTakeItem();
            StartCoroutine(Fade());
        }
    }
}