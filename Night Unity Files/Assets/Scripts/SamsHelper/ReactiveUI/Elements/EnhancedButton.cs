using System;
using System.Collections;
using System.Collections.Generic;
using Facilitating.UI.Elements;
using SamsHelper.Input;
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
            return _button;
        }

        public void Awake()
        {
            InputHandler.RegisterInputListener(this);
            _button = GetComponent<Button>();
            _textChildren = Helper.FindAllComponentsInChildren<EnhancedText>(transform);
            _imageChildren = Helper.FindAllComponentsInChildren<Image>(transform);
            _buttonClickSource = Camera.main.GetComponent<AudioSource>();
            if (Border != null) Border.SetActive(false);
        }

        private void Enter()
        {
            if (UseGlobalColours) UseSelectedColours();
            OnSelectActions?.Invoke();
            _buttonClickSource.Play();
        }

        private void Exit()
        {
            if (UseGlobalColours) UseDeselectedColours();
            OnDeselectActions?.Invoke();
        }

        public void AddOnClick(UnityAction a)
        {
            _button.onClick.AddListener(a);
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
            _fadeCoroutine = StartCoroutine(Fade(target));
        }

        private IEnumerator Fade(float targetVal)
        {
            float alpha = 1 - targetVal;
            foreach (Transform child in Helper.FindAllChildren(Border.transform))
            {
                child.GetComponent<Image>().color = new Color(1, 1, 1, alpha);
            }
            for (float t = 0; t < 1; t += Time.deltaTime / FadeDuration)
            {
                Color newColor = new Color(1, 1, 1, Mathf.Lerp(alpha, targetVal, t));
                foreach (Transform child in Helper.FindAllChildren(Border.transform))
                {
                    child.GetComponent<Image>().color = newColor;
                }
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

        public void OnInputDown(InputAxis axis, bool isHeld, float direction = 0)
        {
            if (_button == null || axis != InputAxis.Submit) return;
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

        public void OnInputUp(InputAxis axis)
        {
        }

        public void OnDoubleTap(InputAxis axis, float direction)
        {
        }

        public void SetRightNavigation(EnhancedButton target)
        {
            ModifyNavigation(target, (o, t) =>
            {
                o.selectOnRight = target.Button();
                t.selectOnLeft = _button;
            });
        }

        public void SetLeftNavigation(EnhancedButton target)
        {
            ModifyNavigation(target, (o, t) =>
            {
                o.selectOnLeft = target.Button();
                t.selectOnRight = _button;
            });
        }

        public void SetUpNavigation(EnhancedButton target)
        {
            ModifyNavigation(target, (o, t) =>
            {
                o.selectOnUp = target.Button();
                t.selectOnDown = _button;
            });
        }

        public void SetDownNavigation(EnhancedButton target)
        {
            ModifyNavigation(target, (o, t) =>
            {
                o.selectOnDown = target.Button();
                t.selectOnUp = _button;
            });
        }

        private void ModifyNavigation(EnhancedButton target, Action<Navigation, Navigation> modifyAction)
        {
            Navigation originNavigation = _button.navigation;
            Navigation targetNavigation = target.Button().navigation;
            modifyAction(originNavigation, targetNavigation);
            _button.navigation = originNavigation;
            target.Button().navigation = targetNavigation;
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
    }
}