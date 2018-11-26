using System.Collections.Generic;
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

    public override void Awake()
    {
        base.Awake();
        _dismantleList = gameObject.FindChildWithName<ListController>("List");
        List<ListElement> listElements = new List<ListElement> {new DismantleElement(), new DismantleElement(), new DismantleElementDetailed(), new DismantleElement(), new DismantleElement()};
        _dismantleList.Initialise(listElements, Dismantle, Close);
        _closeButton = gameObject.FindChildWithName<CloseButtonController>("Close Button");
        _closeButton.SetOnClick(Close);
        _closeButton.SetCallback(Close);
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
        string dismantleText = "In sacrificing your\n";
        dismantleText += "<size=40>" + gear.Name + "</size>\n";
        dismantleText += "You will receive\n\n";
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

    private class DismantleElementDetailed : ListElement
    {
        private EnhancedText _nameText, _receiveText;

        protected override void UpdateCentreItemEmpty()
        {
            _nameText.SetText("No Items found to sacrifice");
            _receiveText.SetText("-");
        }

        public override void SetColour(Color colour)
        {
            _nameText.SetColor(colour);
            _receiveText.SetColor(colour);
        }

        protected override void SetVisible(bool visible)
        {
            if (visible) return;
            _nameText.SetText("");
            _receiveText.SetText("");
        }

        protected override void CacheUiElements(Transform transform)
        {
            _nameText = transform.FindChildWithName<EnhancedText>("Name");
            _receiveText = transform.FindChildWithName<EnhancedText>("Receive");
        }

        protected override void Update(object o, bool isCentreItem)
        {
            GearItem item = (GearItem) o;
            _nameText.SetText(item.Name);
            _receiveText.SetText(GetDismantleText(item));
        }
    }

    private static List<object> GetDismantleItems()
    {
        List<object> items = Inventory.GetAvailableWeapons().ToObjectList();
        items.Append(Inventory.GetAvailableArmour().ToObjectList());
        items.Append(Inventory.GetAvailableAccessories().ToObjectList());
        items.Append(Inventory.Inscriptions.ToObjectList());
        return items;
    }

    public void Dismantle(object o)
    {
        GearItem gear = (GearItem) o;
        foreach (string reward in _dismantleRewards.Keys)
        {
            int quantity = _dismantleRewards[reward];
            Inventory.IncrementResource(reward, quantity);
        }

        gear.UnEquip();
        if (gear is Weapon) Inventory.Destroy((Weapon) gear);
        else if (gear is Accessory) Inventory.Destroy((Accessory) gear);
        else if (gear is Armour) Inventory.Destroy((Armour) gear);
        else if (gear is Inscription) Inventory.Destroy((Inscription) gear);
    }

    public override void Enter()
    {
        base.Enter();
        CombatManager.Pause();
        DOTween.defaultTimeScaleIndependent = true;
        _dismantleList.Show(GetDismantleItems);
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