﻿using System.Xml;
using DefaultNamespace;
using Facilitating.Persistence;
using Game.Global;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;

namespace Facilitating.UIControllers.Inventories
{
    public abstract class UiInventoryMenuController : MonoBehaviour
    {
        public void Awake()
        {
            CacheElements();
            Initialise();
            Hide();
        }

        public void Show()
        {
            gameObject.SetActive(true);
            AudioController.FadeInMuffle();
            OnShow();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            AudioController.FadeOutMuffle();
            OnHide();
        }

        protected virtual void OnShow()
        {
            UiGearMenuController.SetCloseButtonAction(UiGearMenuController.Close);
        }

        protected virtual void OnHide()
        {
        }

        protected abstract void CacheElements();
        protected abstract void Initialise();

        public abstract bool Unlocked();
        protected abstract void SetUnlocked(bool unlocked);

        public void Load(XmlNode root)
        {
            SetUnlocked(root.BoolFromNode(nameof(GetType)));
        }

        public void Save(XmlNode root)
        {
            root.CreateChild(nameof(GetType), Unlocked());
        }
        
        protected abstract class BasicListElement : ListElement
        {
            protected EnhancedText CentreText;
            protected EnhancedText LeftText;
            protected EnhancedText RightText;

            protected override void SetVisible(bool visible)
            {
                CentreText.gameObject.SetActive(visible);
                LeftText.gameObject.SetActive(visible);
                RightText.gameObject.SetActive(visible);
            }

            protected override void CacheUiElements(Transform transform)
            {
                CentreText = transform.gameObject.FindChildWithName<EnhancedText>("Name");
                LeftText = transform.gameObject.FindChildWithName<EnhancedText>("Type");
                RightText = transform.gameObject.FindChildWithName<EnhancedText>("Dps");
            }

            public override void SetColour(Color c)
            {
                CentreText.SetColor(c);
                LeftText.SetColor(c);
                RightText.SetColor(c);
            }
        }
    }
}