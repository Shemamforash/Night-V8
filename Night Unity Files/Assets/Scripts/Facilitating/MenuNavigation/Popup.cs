using System;
using System.Collections.Generic;
using System.Resources;
using Game.World;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.ReactiveUI.InventoryUI;
using SamsHelper.ReactiveUI.MenuSystem;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Facilitating.MenuNavigation
{
    public class Popup
    {
        private readonly GameObject _popupObject;
        private readonly Transform _container;
        private readonly GameObject _previousSelectable;
        private static string _optionPrefabName = "Prefabs/Button Border";
        private readonly List<GameObject> _options = new List<GameObject>();
        private static Stack<Popup> _popupStack = new Stack<Popup>();
        private Popup _previousPopup;

        public Popup(string title)
        {
            _previousPopup = _popupStack.Count == 0 ? null : _popupStack.Peek();
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
            Helper.FindChildWithName<TextMeshProUGUI>(_popupObject, "Title").text = title;
            _popupStack.Push(this);
            Helper.PrintList(_popupStack.ToArray());
        }

        private void Destroy(bool clearStack = false)
        {
            GameObject.Destroy(_popupObject);
            _previousSelectable.GetComponent<Selectable>().Select();
            Popup thisPopup = _popupStack.Pop();
            if (thisPopup != this)
            {
                throw new Exception("Next popup in popup stack should have been this element, but ,uh, it wasn't.");
            }
            if (_previousPopup == null)
            {
                MenuStateMachine.ShowCurrentMenu();
                WorldState.UnPause();
            }
            else
            {
                if (clearStack)
                {
                    _previousPopup.Destroy(true);
                }
                _previousPopup.Show();
            }
        }

        public void AddBackButton(string title = "Cancel")
        {
            AddButton(title, null, true);
        }

        public void AddList(List<MyGameObject> items, Action<MyGameObject> callback, bool autoDestruct = false)
        {
            MenuList menuList = Helper.InstantiateUiObject("Prefabs/Simple Menu", _container).GetComponent<MenuList>();
            menuList.SetItems(items);
            menuList.GetItems().ForEach(item =>
            {
                item.OnActionPress(() =>
                {
                    if(autoDestruct) Destroy();
                    callback(item.GetLinkedObject());
                });
            });
        }

        public void AddButton(string optionText, Action optionOnClick = null, bool autoDestruct = false)
        {
            GameObject newOption = Helper.InstantiateUiObject(_optionPrefabName, _container);
            Button b = newOption.GetComponent<Button>();
            b.onClick.AddListener(() =>
            {
                if(autoDestruct) Destroy();
                optionOnClick?.Invoke();
            });
            if (_options.Count == 0)
            {
                b.Select();
            }
            Helper.FindChildWithName<TextMeshProUGUI>(newOption, "Text").text = optionText;
            _options.Add(newOption);
            int i = _options.IndexOf(newOption);
            if (i > 0)
            {
                Helper.SetReciprocalNavigation(_options[i - 1], newOption);
            }
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