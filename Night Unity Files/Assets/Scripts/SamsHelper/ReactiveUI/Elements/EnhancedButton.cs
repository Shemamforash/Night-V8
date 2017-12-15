using System;
using System.Collections;
using System.Collections.Generic;
using Facilitating.UI.Elements;
using SamsHelper.Input;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SamsHelper.ReactiveUI.Elements
{
    [RequireComponent(typeof(Button))]
    public class EnhancedButton : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerEnterHandler, IPointerExitHandler, IInputListener
    {
        private event Action OnSelectActions;
        private event Action OnDeselectActions;
        private readonly List<HoldAction> _onHoldActions = new List<HoldAction>();
        private List<EnhancedText> _textChildren = new List<EnhancedText>();
        private List<Image> _imageChildren = new List<Image>();
        private Button _button;
        public GameObject Border;
        [SerializeField] private bool _useBorder = true;
        [Range(0f, 5f)] public float FadeDuration = 0.5f;
        public bool UseGlobalColours = true;
        private AudioSource _buttonClickSource;
        private bool _justEntered;

        private class HoldAction
        {
            private readonly Action _holdAction;
            private readonly float _duration;
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

        public Button Button()
        {
            return _button == null ? GetComponent<Button>() : _button;
        }

        public void Awake()
        {
            InputHandler.RegisterInputListener(this);
            _button = GetComponent<Button>();
            _textChildren = Helper.FindAllComponentsInChildren<EnhancedText>(transform);
            _imageChildren = Helper.FindAllComponentsInChildren<Image>(transform);
            _buttonClickSource = Camera.main.GetComponent<AudioSource>();
            if (Border != null)
            {
                Border.SetActive(false);
                _borderImages.AddRange(Helper.FindAllComponentsInChildren<Image>(Border.transform));
            }
        }

        private void Enter()
        {
            if (UseGlobalColours) UseSelectedColours();
            OnSelectActions?.Invoke();
            _buttonClickSource.Play();
            _justEntered = true;
        }

        private void Exit()
        {
            if (UseGlobalColours) UseDeselectedColours();
            OnDeselectActions?.Invoke();
            if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
            SetBorderColor(new Color(1, 1, 1, 0f));
        }

        public void AddOnClick(UnityAction a)
        {
            GetComponent<Button>().onClick.AddListener(a);
        }

        public void AddOnSelectEvent(Action a) => OnSelectActions += a;
        public void AddOnDeselectEvent(Action a) => OnDeselectActions += a;
        public void OnSelect(BaseEventData eventData) => Enter();
        public void OnDeselect(BaseEventData eventData) => Exit();
        public void OnPointerEnter(PointerEventData p) => Enter();
        public void OnPointerExit(PointerEventData p) => Exit();
        public void AddOnHold(Action a, float duration) => _onHoldActions.Add(new HoldAction(a, duration));

        public void DisableBorder() => _useBorder = false;

        private void UseSelectedColours()
        {
            if (Border != null && _useBorder)
            {
                Border.SetActive(true);
                TryStartFade(1);
            }
            else
            {
                SetColor(UiAppearanceController.Instance.SecondaryColor);
                if (_button.image != null)
                {
                    _button.image.color = UiAppearanceController.Instance.MainColor;
                }
            }
        }

        private Coroutine _fadeCoroutine;

        private void TryStartFade(int target)
        {
            if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
            if(gameObject.activeInHierarchy) _fadeCoroutine = StartCoroutine(Fade(target));
        }

        private readonly List<Image> _borderImages = new List<Image>();

        private void SetBorderColor(Color c)
        {
            _borderImages.ForEach(b => b.color = c);
        }

        private IEnumerator Fade(float targetVal)
        {
            float alpha = 1 - targetVal;
            SetBorderColor(new Color(1, 1, 1, alpha));
            for (float t = 0; t < 1; t += Time.deltaTime / FadeDuration)
            {
                SetBorderColor(new Color(1, 1, 1, Mathf.Lerp(alpha, targetVal, t)));
                yield return null;
            }
        }

        private void UseDeselectedColours()
        {
            if (Border != null && _useBorder)
            {
                Border.SetActive(false);
                TryStartFade(0);
            }
            else
            {
                SetColor(UiAppearanceController.Instance.MainColor);
                if (_button.image != null)
                {
                    _button.image.color = UiAppearanceController.Instance.BackgroundColor;
                }
            }
        }

        public void SetColor(Color c)
        {
            _textChildren.ForEach(t => t.SetColor(c));
            _imageChildren.ForEach(i => i.color = c);
        }

        private bool IsSelected()
        {
            return EventSystem.current.currentSelectedGameObject == gameObject;
        }

        public void OnInputDown(InputAxis axis, bool isHeld, float direction = 0)
        {
            if (_button == null || !IsSelected()) return;
            if (axis == InputAxis.Submit)
            {
                if (isHeld)
                {
                    if (_button.gameObject == EventSystem.current.currentSelectedGameObject)
                    {
                        _onHoldActions.ForEach(a => a.ExecuteIfDone());
                    }
                }
                else
                {
                    _onHoldActions.ForEach(a => a.Reset());
                }
            }
            else if (axis == InputAxis.Vertical && !isHeld && !_justEntered)
            {
                if (direction > 0)
                {
                    OnUpAction?.Invoke();
                    Exit();
                }
                else
                {
                    OnDownAction?.Invoke();
                    Exit();
                }
            }
        }

        public void OnInputUp(InputAxis axis)
        {
            if (IsSelected() && axis == InputAxis.Vertical)
            {
                _justEntered = false;
            }
        }

        public void OnDoubleTap(InputAxis axis, float direction)
        {
        }

        public void SetRightNavigation(EnhancedButton target)
        {
            if (target == null) return;

            Navigation navigation = GetNavigation();
            navigation.selectOnRight = target.Button();
            SetNavigation(navigation);

            navigation = target.GetNavigation();
            navigation.selectOnLeft = _button;
            target.SetNavigation(navigation);
        }

        public void SetLeftNavigation(EnhancedButton target)
        {
            if (target == null) return;

            Navigation navigation = GetNavigation();
            navigation.selectOnLeft = target.Button();
            SetNavigation(navigation);

            navigation = target.GetNavigation();
            navigation.selectOnRight = _button;
            target.SetNavigation(navigation);
        }

        public void SetUpNavigation(EnhancedButton target)
        {
            if (target == null) return;

            Navigation navigation = GetNavigation();
            navigation.selectOnUp = target.Button();
            SetNavigation(navigation);

            navigation = target.GetNavigation();
            navigation.selectOnDown = _button;
            target.SetNavigation(navigation);
        }

        public void SetDownNavigation(EnhancedButton target)
        {
            if (target == null) return;

            Navigation navigation = GetNavigation();
            navigation.selectOnDown = target.Button();
            SetNavigation(navigation);

            navigation = target.GetNavigation();
            navigation.selectOnUp = _button;
            target.SetNavigation(navigation);
        }

        private Navigation GetNavigation()
        {
            return _button.navigation;
        }

        private void SetNavigation(Navigation navigation)
        {
            _button.navigation = navigation;
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

        private Action OnDownAction, OnUpAction;

        public void SetOnDownAction(Action a)
        {
            OnDownAction = a;
        }

        public void SetOnUpAction(Action a)
        {
            OnUpAction = a;
        }
    }
}