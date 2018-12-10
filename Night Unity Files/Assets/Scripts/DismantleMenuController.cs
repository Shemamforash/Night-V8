﻿using System.Collections.Generic;
using System.Linq;
using DefaultNamespace;
using DG.Tweening;
using Game.Combat.Generation;
using Game.Gear;
using Game.Gear.Armour;
using Game.Gear.Weapons;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using SamsHelper.ReactiveUI.MenuSystem;
using TMPro;
using UnityEngine;

public class DismantleMenuController : Menu
{
    private ListController _dismantleList;
    private static readonly Dictionary<string, int> _dismantleRewards = new Dictionary<string, int>();
    private CloseButtonController _closeButton;
    private GameObject _dismantledScreen;
    private EnhancedText _receivedText;

    public override void Awake()
    {
        base.Awake();
        _dismantleList = gameObject.FindChildWithName<ListController>("List");
        _dismantleList.Initialise(typeof(DismantleElement), Dismantle, Close, GetDismantleItems);
        _closeButton = gameObject.FindChildWithName<CloseButtonController>("Close Button");
        _closeButton.SetOnClick(Close);
        _closeButton.SetCallback(Close);

        _dismantledScreen = gameObject.FindChildWithName("Dismantled");
        _receivedText = _dismantledScreen.FindChildWithName<EnhancedText>("Receive");
        _dismantledScreen.FindChildWithName<EnhancedButton>("Button").AddOnClick(Show);
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
        int quality = (int) gear.Quality();
        if (gear is Inscription) CalculateInscriptionReward(quality);
        else if (gear is Weapon) CalculateWeaponReward(quality);
        else if (gear is Armour) CalculateArmourReward(quality);
        else if (gear is Accessory) CalculateAccessoryReward(quality);
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
            if (i > 0) possibleRewards.Add("Scrap");
            if (i > 1) possibleRewards.Add("Metal");
            if (i > 2) possibleRewards.Add("Meteor");
            if (i > 3) possibleRewards.Add("Alloy");
        }

        if (possibleRewards.Count == 0) return;
        AddReward(possibleRewards.RandomElement(), 1);
        AddReward(possibleRewards.RandomElement(), 1);
    }

    private static void CalculateArmourReward(int quality)
    {
        AddReward("Salt", quality);
        List<string> possibleRewards = new List<string>();
        for (int i = 0; i < quality; ++i)
        {
            if (i > 0) possibleRewards.Add("Scrap");
            if (i > 1) possibleRewards.Add("Skin");
            if (i > 2) possibleRewards.Add("Leather");
            if (i > 3) possibleRewards.Add("Meteor");
            if (i > 4) possibleRewards.Add("Alloy");
        }

        AddReward(possibleRewards.RandomElement(), 1);
        AddReward(possibleRewards.RandomElement(), 1);
    }

    private static void CalculateAccessoryReward(int quality)
    {
        AddReward("Salt", quality);
        AddReward("Essence", quality);
        List<string> possibleRewards = new List<string>();
        for (int i = 0; i < quality; ++i)
        {
            if (i > 0) possibleRewards.Add("Scrap");
            if (i > 1)
            {
                possibleRewards.Add("Skin");
                possibleRewards.Add("Metal");
            }

            if (i > 2)
            {
                possibleRewards.Add("Leather");
                possibleRewards.Add("Meteor");
            }

            if (i > 3)
            {
                possibleRewards.Add("Alloy");
            }
        }

        AddReward(possibleRewards.RandomElement(), 1);
        AddReward(possibleRewards.RandomElement(), 1);
        AddReward(possibleRewards.RandomElement(), 1);
    }

    private class DismantleElement : ListElement
    {
        private EnhancedText _text;

        protected override void UpdateCentreItemEmpty()
        {
            _text.SetText("No Items found to sacrifice");
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
        }
    }

    private static List<object> GetDismantleItems()
    {
        List<object> items = Inventory.GetAvailableWeapons().ToObjectList();
        items.AddRange(Inventory.GetAvailableArmour().ToObjectList());
        items.AddRange(Inventory.GetAvailableAccessories().ToObjectList());
        items.AddRange(Inventory.Inscriptions.ToObjectList());
        return items;
    }

    private void Dismantle(object o)
    {
        GearItem gear = (GearItem) o;
        foreach (string reward in _dismantleRewards.Keys)
        {
            int quantity = _dismantleRewards[reward];
            Inventory.IncrementResource(reward, quantity);
        }

        gear.UnEquip();
        ShowDismantledScreen(o);
        if (gear is Weapon) Inventory.Destroy((Weapon) gear);
        else if (gear is Accessory) Inventory.Destroy((Accessory) gear);
        else if (gear is Armour) Inventory.Destroy((Armour) gear);
        else if (gear is Inscription) Inventory.Destroy((Inscription) gear);
    }

    private void ShowDismantledScreen(object o)
    {
        GearItem gear = (GearItem) o;
        _dismantledScreen.SetActive(true);
        _dismantleList.gameObject.SetActive(false);
        _receivedText.SetText(GetDismantleText(gear));
    }

    public override void Enter()
    {
        base.Enter();
        CombatManager.Pause();
        DOTween.defaultTimeScaleIndependent = true;
        _dismantledScreen.SetActive(false);
        _dismantleList.gameObject.SetActive(true);
        _dismantleList.Show();
    }

    public static void Show()
    {
        MenuStateMachine.ShowMenu("Dismantle Menu");
    }

    public void Close()
    {
        _closeButton.Flash();
        _dismantleList.Hide();
        CombatManager.Resume();
        MenuStateMachine.ReturnToDefault();
    }
}