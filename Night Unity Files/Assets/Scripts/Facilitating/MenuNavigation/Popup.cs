using System;
using System.Collections.Generic;
using Game.Global;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.MenuSystem;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Facilitating.MenuNavigation
{
    public class Popup
    {
        private static readonly string _optionPrefabName = "Prefabs/Inventory/SimpleItem";
        private static readonly Stack<Popup> PopupStack = new Stack<Popup>();
        private readonly Transform _container;
        private readonly List<GameObject> _options = new List<GameObject>();
        private readonly GameObject _popupObject;
        private readonly Popup _previousPopup;
        private readonly GameObject _previousSelectable;

        public Popup(string title)
        {
            _previousPopup = PopupStack.Count == 0 ? null : PopupStack.Peek();
            if (_previousPopup == null)
            {
                MenuStateMachine.HideCurrentMenu();
                WorldState.Pause();
            }
            else
            {
                _previousPopup.Hide();
            }

            _popupObject = Helper.InstantiateUiObject("Prefabs/Popup Menu", GameObject.Find("Canvas").transform);
            _container = _popupObject.transform.Find("Bar");
            _previousSelectable = EventSystem.current.currentSelectedGameObject;
            _popupObject.FindChildWithName<TextMeshProUGUI>("Title").text = title;
            PopupStack.Push(this);
        }

        private void Destroy(bool clearStack = false)
        {
            Object.Destroy(_popupObject);
            _previousSelectable.GetComponent<Selectable>().Select();
            Popup thisPopup = PopupStack.Pop();
            if (thisPopup != this) throw new Exception("Next popup in popup stack should have been this element, but ,uh, it wasn't.");
            if (_previousPopup == null)
            {
                MenuStateMachine.ShowCurrentMenu();
                WorldState.UnPause();
            }
            else
            {
                if (clearStack) _previousPopup.Destroy(true);
                _previousPopup.Show();
            }
        }

        public void AddBackButton(string title = "Cancel")
        {
            AddButton(title, null, true);
        }

        public void AddButton(string optionText, Action optionOnClick = null, bool autoDestruct = false, bool closeAll = false)
        {
            GameObject newOption = Helper.InstantiateUiObject(_optionPrefabName, _container);
            Button b = newOption.GetComponent<Button>();
            b.onClick.AddListener(() =>
            {
                if (autoDestruct) Destroy(closeAll);
                optionOnClick?.Invoke();
            });
            if (_options.Count == 0) b.Select();
            newOption.FindChildWithName<TextMeshProUGUI>("Text").text = optionText;
            _options.Add(newOption);
            int i = _options.IndexOf(newOption);
            if (i > 0)
            {
                Helper.SetReciprocalNavigation(b, _options[0].GetComponent<Button>());
                Helper.SetReciprocalNavigation(_options[i - 1].GetComponent<Button>(), b);
            }

            b.Select();
        }

        private void Hide()
        {
            _popupObject.SetActive(false);
        }

        private void Show()
        {
            _popupObject.SetActive(true);
        }
    }
}