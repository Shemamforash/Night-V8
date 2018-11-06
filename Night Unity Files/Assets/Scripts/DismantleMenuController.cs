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
    private GameObject _resourceMenuObject;
    private TextMeshProUGUI _offeringText;
    private GearItem _objectToDismantle;
    private EnhancedButton _dismantleButton;
    private readonly Dictionary<string, int> _dismantleRewards = new Dictionary<string, int>();
    private CloseButtonController _closeButton;

    public override void Awake()
    {
        base.Awake();
        _resourceMenuObject = gameObject.FindChildWithName("Resource");
        _offeringText = _resourceMenuObject.FindChildWithName<TextMeshProUGUI>("Gain Text");
        _dismantleList = gameObject.FindChildWithName<ListController>("List");
        _dismantleButton = gameObject.FindChildWithName<EnhancedButton>("Sacrifice");
        _dismantleList.Initialise(typeof(DismantleElement), ShowDismantleMenu, Close);
        _closeButton = gameObject.FindChildWithName<CloseButtonController>("Close Button");
    }

    private void AddReward(string reward, int quantity)
    {
        if (!_dismantleRewards.ContainsKey(reward))
        {
            _dismantleRewards.Add(reward, 0);
        }

        quantity = _dismantleRewards[reward] + quantity;
        _dismantleRewards[reward] = quantity;
    }

    private void CalculateDismantleRewards()
    {
        _dismantleRewards.Clear();
        int quality = (int) _objectToDismantle.Quality();
        if (_objectToDismantle is Inscription) CalculateInscriptionReward(quality);
        else if (_objectToDismantle is Weapon) CalculateWeaponReward(quality);
        else if (_objectToDismantle is Armour) CalculateArmourReward(quality);
        else if (_objectToDismantle is Accessory) CalculateAccessoryReward(quality);
        DisplayDismantleText();
    }

    private void DisplayDismantleText()
    {
        string dismantleText = "In sacrificing your\n";
        dismantleText += "<size=40>" + _objectToDismantle.Name + "</size>\n";
        dismantleText += "You will receive\n\n";
        foreach (string reward in _dismantleRewards.Keys)
        {
            int quantity = _dismantleRewards[reward];
            dismantleText += quantity + "x " + reward + "\n";
        }

        _offeringText.text = dismantleText;
    }

    private void CalculateInscriptionReward(int quality)
    {
        AddReward("Essence", 5 * quality);
        if (Helper.RollDie(0, 6)) AddReward("Radiance", 1);
    }

    private void CalculateWeaponReward(int quality)
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

        AddReward(possibleRewards.RandomElement(), 1);
        AddReward(possibleRewards.RandomElement(), 1);
    }

    private void CalculateArmourReward(int quality)
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

    private void CalculateAccessoryReward(int quality)
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

        protected override void Update(object o)
        {
            GearItem item = (GearItem) o;
            string name = item.Name;
            if (item.EquippedCharacter != null) name += " (E)";
            _text.SetText(name);
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

    public void Dismantle()
    {
        foreach (string reward in _dismantleRewards.Keys)
        {
            int quantity = _dismantleRewards[reward];
            Inventory.IncrementResource(reward, quantity);
        }

        _objectToDismantle.Unequip();
        if (_objectToDismantle is Weapon) Inventory.Destroy((Weapon) _objectToDismantle);
        else if (_objectToDismantle is Accessory) Inventory.Destroy((Accessory) _objectToDismantle);
        else if (_objectToDismantle is Armour) Inventory.Destroy((Armour) _objectToDismantle);
        else if (_objectToDismantle is Inscription) Inventory.Destroy((Inscription) _objectToDismantle);
        Cancel();
    }

    private void ShowList()
    {
        _resourceMenuObject.SetActive(false);
        _dismantleList.Show(GetDismantleItems);
    }

    private void ShowDismantleMenu(object o)
    {
        _objectToDismantle = (GearItem) o;
        _dismantleList.Hide();
        _resourceMenuObject.SetActive(true);
        _dismantleButton.Select();
        CalculateDismantleRewards();
    }

    public void Cancel()
    {
        ShowList();
    }

    public override void Enter()
    {
        base.Enter();
        CombatManager.Pause();
        DOTween.defaultTimeScaleIndependent = true;
        ShowList();
    }

    public static void Show()
    {
        MenuStateMachine.ShowMenu("Dismantle Menu");
    }

    public void Close()
    {
        _closeButton.Flash();
        CombatManager.Unpause();
        MenuStateMachine.ReturnToDefault();
    }
}