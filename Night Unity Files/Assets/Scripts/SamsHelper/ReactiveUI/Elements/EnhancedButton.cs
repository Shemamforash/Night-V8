using System;
using System.Collections;
using System.Collections.Generic;
using SamsHelper.Input;
using SamsHelper.Libraries;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace SamsHelper.ReactiveUI.Elements
{
    [RequireComponent(typeof(Button))]
    public class EnhancedButton : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerEnterHandler, IPointerExitHandler, IInputListener
    {
        private const float FadeTime = 0.25f;
        private readonly List<Image> _buttonImages = new List<Image>();
        private readonly List<HoldAction> _onHoldActions = new List<HoldAction>();
        private float _borderAlpha;
        private Button _button;
        private Coroutine _fadeIn, _fadeOut;
        private bool _justEntered;
        private Action _onDownAction, _onUpAction;
        private static GameObject _borderPrefab;
        private static GameObject _basicBorderPrefab;
        private GameObject _border;
        private event Action OnSelectActions;
        private event Action OnDeselectActions;
        public bool UseAdvancedBorder;
        private bool _isSelected;
        private AudioClip[] _buttonSelectClip;
        private AudioSource _audioSource;
        private AudioMixerGroup _modifiedMixerGroup;

        public void OnDeselect(BaseEventData eventData)
        {
            Exit();
        }

        public void OnInputDown(InputAxis axis, bool isHeld, float direction = 0)
        {
            if (_button == null || !IsSelected()) return;
            if (axis == InputAxis.Fire)
                if (isHeld)
                {
                    if (_button.gameObject == EventSystem.current.currentSelectedGameObject) _onHoldActions.ForEach(a => a.ExecuteIfDone());
                }
                else
                {
                    _onHoldActions.ForEach(a => a.Reset());
                }

            if (isHeld || _justEntered || axis != InputAxis.Vertical) return;
            if (direction > 0)
            {
                if (_button.navigation.selectOnUp == null || _onUpAction == null) return;
                _onUpAction?.Invoke();
                Exit();
            }
            else if (direction < 0)
            {
                if (_button.navigation.selectOnDown == null || _onDownAction == null) return;
                _onDownAction?.Invoke();
                Exit();
            }
        }

        public void OnInputUp(InputAxis axis)
        {
            if (axis == InputAxis.Vertical && IsSelected()) _justEntered = false;
        }

        public void OnDoubleTap(InputAxis axis, float direction)
        {
        }

        public void OnPointerEnter(PointerEventData p)
        {
            Enter();
        }

        public void OnPointerExit(PointerEventData p)
        {
            Exit();
        }

        public void OnSelect(BaseEventData eventData)
        {
            Enter();
        }

        public void OnDisable()
        {
            _borderAlpha = 0f;
            UpdateBorderAlpha();
        }

        public Button Button()
        {
            if (_button == null) _button = GetComponent<Button>();
            return _button;
        }

        public void Awake()
        {
            InputHandler.RegisterInputListener(this);
            _button = GetComponent<Button>();
            if (_borderPrefab == null)
            {
                _borderPrefab = Resources.Load<GameObject>("Prefabs/Borders/Border");
                _basicBorderPrefab = Resources.Load<GameObject>("Prefabs/Borders/Border Basic");
            }

            _border = Instantiate(UseAdvancedBorder ? _borderPrefab : _basicBorderPrefab);
            _border.transform.SetParent(transform, false);
            RectTransform rect = _border.GetComponent<RectTransform>();
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            if (_border != null)
            {
                _buttonImages.AddRange(Helper.FindAllComponentsInChildren<Image>(_border.transform));
                Image i = _border.GetComponent<Image>();
                if (i != null) _buttonImages.Add(i);
            }
            else _buttonImages.Add(GetComponent<Image>());

            _borderAlpha = 0f;
            UpdateBorderAlpha();
        }

        private void Enter()
        {
            OnSelectActions?.Invoke();
            _justEntered = true;
            _fadeIn = StartCoroutine(FadeIn());
            _isSelected = true;
            PlayButtonSelectSound();
        }

        public void PlayButtonSelectSound()
        {
            if (_buttonSelectClip == null) _buttonSelectClip = Resources.LoadAll<AudioClip>("Sounds/Button Clicks");
            if (_audioSource == null)
            {
                _audioSource = Camera.main.gameObject.GetComponent<AudioSource>();
                if (_audioSource == null) _audioSource = Camera.main.gameObject.AddComponent<AudioSource>();
                if (_modifiedMixerGroup == null) _modifiedMixerGroup = Resources.Load<AudioMixer>("AudioMixer/Master").FindMatchingGroups("Modified")[0];
                _audioSource.outputAudioMixerGroup = _modifiedMixerGroup;
            }

            _audioSource.pitch = Random.Range(0.85f, 1f);
            _audioSource.PlayOneShot(_buttonSelectClip.RandomElement());
        }

        private void Exit()
        {
            OnDeselectActions?.Invoke();
            _isSelected = false;
            if (!gameObject.activeInHierarchy) return;
            _fadeOut = StartCoroutine(FadeOut());
        }

        public void AddOnClick(UnityAction a)
        {
            GetComponent<Button>().onClick.AddListener(a);
        }

        public void AddOnSelectEvent(Action a)
        {
            OnSelectActions += a;
        }

        public void AddOnDeselectEvent(Action a)
        {
            OnDeselectActions += a;
        }

        public void AddOnHold(Action a, float duration)
        {
            _onHoldActions.Add(new HoldAction(a, duration));
        }

        private void UpdateBorderAlpha()
        {
            _buttonImages.ForEach(b => b.color = new Color(1, 1, 1, _borderAlpha));
        }

        private IEnumerator FadeIn()
        {
            if (_fadeOut != null) StopCoroutine(_fadeOut);
            while (_borderAlpha < 1)
            {
                _borderAlpha += 1f / FadeTime * Time.deltaTime;
                if (_borderAlpha >= 1) _borderAlpha = 1;
                UpdateBorderAlpha();
                yield return null;
            }
        }

        private IEnumerator FadeOut()
        {
            if (_fadeIn != null) StopCoroutine(_fadeIn);
            while (_borderAlpha > 0)
            {
                _borderAlpha -= 1f / FadeTime * Time.deltaTime;
                if (_borderAlpha <= 0) _borderAlpha = 0;
                UpdateBorderAlpha();
                yield return null;
            }
        }

        private bool IsSelected()
        {
//            return EventSystem.current.currentSelectedGameObject == gameObject;
            return _isSelected;
        }

        private void OnDestroy()
        {
            InputHandler.UnregisterInputListener(this);
        }

        public void SetRightNavigation(EnhancedButton target, bool reciprocate = true)
        {
            if (target == null) return;

            Navigation navigation = GetNavigation();
            navigation.selectOnRight = target.Button();
            SetNavigation(navigation);

            if (!reciprocate) return;
            navigation = target.GetNavigation();
            navigation.selectOnLeft = _button;
            target.SetNavigation(navigation);
        }

        public void SetLeftNavigation(EnhancedButton target, bool reciprocate = true)
        {
            if (target == null) return;

            Navigation navigation = GetNavigation();
            navigation.selectOnLeft = target.Button();
            SetNavigation(navigation);

            if (!reciprocate) return;
            navigation = target.GetNavigation();
            navigation.selectOnRight = _button;
            target.SetNavigation(navigation);
        }

        public void SetUpNavigation(EnhancedButton target, bool reciprocate = true)
        {
            if (target == null) return;

            Navigation navigation = GetNavigation();
            navigation.selectOnUp = target.Button();
            SetNavigation(navigation);

            if (!reciprocate) return;
            navigation = target.GetNavigation();
            navigation.selectOnDown = _button;
            target.SetNavigation(navigation);
        }

        public void SetDownNavigation(EnhancedButton target, bool reciprocate = true)
        {
            Navigation navigation = GetNavigation();
            navigation.selectOnDown = target == null ? null : target.Button();
            SetNavigation(navigation);
            if (target == null || !reciprocate) return;
            navigation = target.GetNavigation();
            navigation.selectOnUp = _button;
            target.SetNavigation(navigation);
        }

        private Navigation GetNavigation()
        {
            return Button().navigation;
        }

        private void SetNavigation(Navigation navigation)
        {
            Button().navigation = navigation;
        }

        public void ClearNavigation()
        {
            Navigation navigation = _button.navigation;
            navigation.selectOnUp = null;
            navigation.selectOnDown = null;
            navigation.selectOnLeft = null;
            navigation.selectOnRight = null;
            _button.navigation = navigation;
        }

        public void SetOnDownAction(Action a)
        {
            _onDownAction = a;
        }

        public void SetOnUpAction(Action a)
        {
            _onUpAction = a;
        }

        private class HoldAction
        {
            private readonly float _duration;
            private readonly Action _holdAction;
            private float _startTime;

            public HoldAction(Action a, float duration)
            {
                _holdAction = a;
                _duration = duration;
                Reset();
            }

            public void Reset()
            {
                _startTime = Time.time;
            }

            public void ExecuteIfDone()
            {
                if (!(Time.time - _startTime > _duration)) return;
                _holdAction();
                Reset();
            }
        }

        public void Select()
        {
            Button().Select();
        }
    }
}