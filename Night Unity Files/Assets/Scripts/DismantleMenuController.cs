using System.Collections.Generic;
using System.Linq;
using DefaultNamespace;
using DG.Tweening;
using Game.Combat.Generation;
using Game.Gear;
using Game.Gear.Armour;
using Game.Gear.Weapons;
using Game.Global;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using SamsHelper.ReactiveUI.MenuSystem;
using TMPro;
using UnityEngine;

public class DismantleMenuController : Menu
{
    private static ListController _dismantleList;
    private static Dictionary<string, int> _dismantleRewards;
    private EnhancedButton _acceptButton;
    private static CloseButtonController _closeButton;
    private static GameObject _dismantledScreen;
    private EnhancedText _receivedText;
    private GearItem _gearToDismantle;

    protected override void Awake()
    {
        base.Awake();
        _dismantleList = gameObject.FindChildWithName<ListController>("List");
        _dismantleList.Initialise(typeof(DismantleElement), ShowDismantledScreen, Close, GetDismantleItems);
        _closeButton = gameObject.FindChildWithName<CloseButtonController>("Close Button");


        _dismantledScreen = gameObject.FindChildWithName("Dismantled");
        _acceptButton = _dismantledScreen.FindChildWithName<EnhancedButton>("Button");
        _acceptButton.AddOnClick(Dismantle);
        _receivedText = _dismantledScreen.FindChildWithName<EnhancedText>("Receive");
    }

    private static string GetDismantleText()
    {
        string dismantleText = "";
        foreach (string reward in _dismantleRewards.Keys)
        {
            int quantity = _dismantleRewards[reward];
            dismantleText += quantity + "x " + reward + "\n";
        }

        return dismantleText;
    }

    private static List<object> GetDismantleItems()
    {
        List<object> items = Inventory.GetAvailableWeapons().ToObjectList();
        items.AddRange(Inventory.GetAvailableAccessories().ToObjectList());
        items.AddRange(Inventory.Inscriptions.ToObjectList());
        return items;
    }

    private void Dismantle()
    {
        foreach (string reward in _dismantleRewards.Keys)
        {
            int quantity = _dismantleRewards[reward];
            Inventory.IncrementResource(reward, quantity);
        }

        _gearToDismantle.UnEquip();
        switch (_gearToDismantle)
        {
            case Weapon weapon:
                Inventory.Destroy(weapon);
                break;
            case Accessory accessory:
                Inventory.Destroy(accessory);
                break;
            case Inscription inscription:
                Inventory.Destroy(inscription);
                break;
        }

        _closeButton.SetOnClick(Close);
        SaveStoneBehaviour.SetUsed();
        Close();
    }

    private void ShowDismantledScreen(object o)
    {
        _closeButton.SetOnClick(ShowDismantleList);
        _gearToDismantle = (GearItem) o;
        _dismantleRewards = _gearToDismantle.GetDismantleRewards();
        _dismantledScreen.SetActive(true);
        _dismantleList.gameObject.SetActive(false);
        _receivedText.SetText(GetDismantleText());
        _acceptButton.Select();
    }

    private static void ShowDismantleList()
    {
        _dismantledScreen.SetActive(false);
        _dismantleList.gameObject.SetActive(true);
        _dismantleList.Show();
    }

    public override void Enter()
    {
        base.Enter();
        _closeButton.SetOnClick(Close);
        WorldState.Pause();
        DOTween.defaultTimeScaleIndependent = true;
        _closeButton.Enable();
        ShowDismantleList();
    }

    public static void Show()
    {
        MenuStateMachine.ShowMenu("Dismantle Menu");
    }

    private void Close()
    {
        _closeButton.Disable();
        _closeButton.Flash();
        _dismantleList.Hide();
        WorldState.Resume();
        MenuStateMachine.ReturnToDefault();
    }

    private class DismantleElement : ListElement
    {
        private EnhancedText _text;

        protected override void UpdateCentreItemEmpty()
        {
            _text.SetText("No Items found to sacrifice");
            _text.FindChildWithName<CanvasGroup>("Button").alpha = 0;
        }

        public override void SetColour(Color colour)
        {
            _text.SetColor(colour);
        }

        protected override void SetVisible(bool visible)
        {
            if (!visible) _text.SetText("");
        }

        protected override void CacheUiElements(Transform transform)
        {
            _text = transform.GetComponent<EnhancedText>();
        }

        protected override void Update(object o, bool isCentreItem)
        {
            GearItem item = (GearItem) o;
            _text.SetText(item.Name);
            if (isCentreItem) _text.FindChildWithName<CanvasGroup>("Button").alpha = 1;
        }
    }
}