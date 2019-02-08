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
    private static readonly Dictionary<string, int> _dismantleRewards = new Dictionary<string, int>();
    private EnhancedButton _acceptButton;
    private static CloseButtonController _closeButton;
    private static GameObject _dismantledScreen;
    private EnhancedText _receivedText;
    private GearItem _gearToDismantle;

    public override void Awake()
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

    private static void AddReward(string reward, int quantity)
    {
        if (!_dismantleRewards.ContainsKey(reward)) _dismantleRewards.Add(reward, 0);
        quantity = _dismantleRewards[reward] + quantity;
        _dismantleRewards[reward] = quantity;
    }

    private static void CalculateDismantleRewards(GearItem gear)
    {
        _dismantleRewards.Clear();
        int quality = (int) gear.Quality() + 1;
        Random.State oldState = Random.state;
        Random.InitState(gear.ID());
        switch (gear)
        {
            case Inscription _:
                CalculateInscriptionReward(quality);
                break;
            case Weapon _:
                CalculateWeaponReward(quality);
                break;
            case Accessory _:
                CalculateAccessoryReward(quality);
                break;
        }

        Random.state = oldState;
    }

    private static string GetDismantleText(GearItem gear)
    {
        CalculateDismantleRewards(gear);
        string dismantleText = "";
        foreach (string reward in _dismantleRewards.Keys)
        {
            int quantity = _dismantleRewards[reward];
            dismantleText += quantity + "x " + reward + "\n";
        }

        return dismantleText;
    }

    private static void CalculateInscriptionReward(int quality)
    {
        AddReward("Essence", 5 * quality);
        if (Helper.RollDie(0, 6)) AddReward("Radiance", 1);
    }

    private static void CalculateWeaponReward(int quality)
    {
        AddReward("Essence", quality);
        List<string> possibleRewards = new List<string>();
        for (int i = 0; i < quality; ++i)
        {
            if (i == 0) possibleRewards.Add("Essence");
            if (i == 1) possibleRewards.Add("Rusty Scrap");
            if (i == 2) possibleRewards.Add("Metal Shards");
            if (i == 3) possibleRewards.Add("Ancient Relics");
            if (i == 4) possibleRewards.Add("Celestial Fragments");
        }

        int count = Mathf.FloorToInt(quality / 2f) + 1;
        for (int i = 0; i < count; ++i) AddReward(possibleRewards.RemoveRandom(), 2);
    }

    private static void CalculateAccessoryReward(int quality)
    {
        AddReward("Salt", quality);
        AddReward("Essence", quality);
        List<string> possibleRewards = new List<string>();
        for (int i = 0; i < quality; ++i)
        {
            if (i == 0) possibleRewards.Add("Essence");
            if (i == 1) possibleRewards.Add("Rusty Scrap");
            if (i == 2) possibleRewards.Add("Metal Shards");
            if (i == 3) possibleRewards.Add("Ancient Relics");
            if (i == 4) possibleRewards.Add("Celestial Shards");
        }

        int count = Mathf.FloorToInt(quality / 2f) + 1;
        for (int i = 0; i < count; ++i) AddReward(possibleRewards.RemoveRandom(), 1);
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
    }

    private void ShowDismantledScreen(object o)
    {
        _closeButton.SetOnClick(Show);
        _closeButton.SetCallback(Show);
        _gearToDismantle = (GearItem) o;
        _dismantledScreen.SetActive(true);
        _dismantleList.gameObject.SetActive(false);
        _receivedText.SetText(GetDismantleText(_gearToDismantle));
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
        _closeButton.SetCallback(Close);
        WorldState.Pause();
        DOTween.defaultTimeScaleIndependent = true;
        _closeButton.Enable();
        ShowDismantleList();
    }

    public static void Show()
    {
        MenuStateMachine.ShowMenu("Dismantle Menu");
        ShowDismantleList();
    }

    public void Close()
    {
        _closeButton.Disable();
        _closeButton.Flash();
        _dismantleList.Hide();
        WorldState.Resume();
        MenuStateMachine.ReturnToDefault();
    }
}