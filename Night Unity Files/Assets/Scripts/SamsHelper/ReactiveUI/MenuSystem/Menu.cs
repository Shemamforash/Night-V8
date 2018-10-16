﻿using DG.Tweening;
using SamsHelper.Input;
using UnityEngine;
using UnityEngine.UI;

namespace SamsHelper.ReactiveUI.MenuSystem
{
    [RequireComponent(typeof(CanvasGroup))]
    public class Menu : MonoBehaviour
    {
        public Selectable DefaultSelectable;

        [HideInInspector] public bool PauseOnOpen = true;

        private CanvasGroup _canvasGroup;

        public virtual void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            if(_canvasGroup == null) Debug.Log(gameObject.name);
        }

        public virtual void Enter()
        {
            _canvasGroup.alpha = 1;
        }

        public virtual void Exit()
        {
            DOTween.defaultTimeScaleIndependent = false;
            _canvasGroup.alpha = 0;
            InputHandler.SetCurrentListener(null);
        }

        public void SetAlpha(float alpha)
        {
            _canvasGroup.alpha = alpha;
        }
    }
}