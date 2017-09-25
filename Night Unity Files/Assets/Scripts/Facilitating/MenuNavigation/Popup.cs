using System;
using System.Collections.Generic;
using Game.World;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.ReactiveUI.InventoryUI;
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
        
        public Popup(string title)
        {
            WorldState.Instance().Pause();
            _popupObject = Helper.InstantiateUiObject("Prefabs/Popup Menu", GameObject.Find("Canvas").transform);
            _container = _popupObject.transform.Find("Bar");
            _previousSelectable = EventSystem.current.currentSelectedGameObject;
            Helper.FindChildWithName<TextMeshProUGUI>(_popupObject, "Title").text = title;
        }

        private void Destroy()
        {
            WorldState.Instance().UnPause();
            GameObject.Destroy(_popupObject);
            _previousSelectable.GetComponent<Selectable>().Select();
        }

        public void AddList(List<MyGameObject> items, Action<MyGameObject> callback)
        {
            MenuList menuList = Helper.InstantiateUiObject("Prefabs/Simple Menu", _container).GetComponent<MenuList>();
            menuList.SetItems(items);
            menuList.GetItems().ForEach(item =>
            {
                item.OnActionPress(() =>
                {
                    Destroy();
                    callback(item.GetLinkedObject());
                });
            });
        }

        public void AddCancelButton()
        {
            AddButton("Cancel", null);
        }
        
        public void AddButton(string optionText, Action optionOnClick)
        {
            GameObject newOption = Helper.InstantiateUiObject(_optionPrefabName, _container);
            Button b = newOption.GetComponent<Button>();
            b.onClick.AddListener(() =>
            {
                Destroy();
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
    }
}