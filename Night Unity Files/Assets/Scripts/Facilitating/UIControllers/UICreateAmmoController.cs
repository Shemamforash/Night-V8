using System;
using System.Collections.Generic;
using System.Net;
using Facilitating.UI.Elements;
using Game.Characters;
using Game.Characters.CharacterActions;
using Game.Characters.Player;
using Game.Gear.Weapons;
using Game.World;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.ReactiveUI.Elements;
using SamsHelper.ReactiveUI.MenuSystem;
using UnityEngine;

public class UICreateAmmoController : Menu
{
    private static Player _player;
    public EnhancedButton PistolButton, RifleButton, ShotgunButton, SmgButton, LmgButton, CloseButton;
    private static UICreateAmmoController _instance;
    public EnhancedText ScrapLeft;
    private List<EnhancedButton> _canAfford;
    private List<EnhancedButton> _cantAfford;

    public void Start()
    {
        PistolButton.AddOnClick(() => CraftAmmo(InventoryResourceType.PistolMag));
        RifleButton.AddOnClick(() => CraftAmmo(InventoryResourceType.RifleMag));
        ShotgunButton.AddOnClick(() => CraftAmmo(InventoryResourceType.ShotgunMag));
        SmgButton.AddOnClick(() => CraftAmmo(InventoryResourceType.SmgMag));
        LmgButton.AddOnClick(() => CraftAmmo(InventoryResourceType.LmgMag));
        CloseButton.AddOnClick(MenuStateMachine.GoToInitialMenu);
        SetAmmoCostText();
        _instance = this;
    }

    private void SetAmmoCostText()
    {
        Helper.FindChildWithName<EnhancedText>(PistolButton.gameObject, "Centre Text").Text(WeaponGenerator.GetWeaponClassWithType(WeaponType.Pistol).AmmoCost + " Scrap");
        Helper.FindChildWithName<EnhancedText>(RifleButton.gameObject, "Centre Text").Text(WeaponGenerator.GetWeaponClassWithType(WeaponType.Rifle).AmmoCost + " Scrap");
        Helper.FindChildWithName<EnhancedText>(ShotgunButton.gameObject, "Centre Text").Text(WeaponGenerator.GetWeaponClassWithType(WeaponType.Shotgun).AmmoCost + " Scrap");
        Helper.FindChildWithName<EnhancedText>(SmgButton.gameObject, "Centre Text").Text(WeaponGenerator.GetWeaponClassWithType(WeaponType.SMG).AmmoCost + " Scrap");
        Helper.FindChildWithName<EnhancedText>(LmgButton.gameObject, "Centre Text").Text(WeaponGenerator.GetWeaponClassWithType(WeaponType.LMG).AmmoCost + " Scrap");
    }

    public void ShowMenu(Player player)
    {
        _player = player;
        _canAfford = new List<EnhancedButton>();
        _cantAfford = new List<EnhancedButton>();
        UpdateScrap();
        UpdateHaveAmounts();

        CheckCanAfford(WeaponType.Pistol, PistolButton);
        CheckCanAfford(WeaponType.Rifle, RifleButton);
        CheckCanAfford(WeaponType.Shotgun, ShotgunButton);
        CheckCanAfford(WeaponType.SMG, SmgButton);
        CheckCanAfford(WeaponType.LMG, LmgButton);

        for (int i = 0; i < _canAfford.Count; ++i)
        {
            _canAfford[i].GetComponent<CanvasGroup>().alpha = 1;
            if (i != 0) _canAfford[i].SetUpNavigation(_canAfford[i - 1]);
        }
        _cantAfford.ForEach(t => t.GetComponent<CanvasGroup>().alpha = 0.4f);
        if (_canAfford.Count > 0)
        {
            DefaultSelectable = _canAfford[0].Button();
            CloseButton.SetUpNavigation(_canAfford[_canAfford.Count - 1]);
        }
        else
        {
            DefaultSelectable = CloseButton.Button();
            CloseButton.SetUpNavigation(null);
        }

        MenuStateMachine.ShowMenu("Create Ammo Menu");
    }

    private void CheckCanAfford(WeaponType type, EnhancedButton button)
    {
        if (WorldState.HomeInventory().GetResourceQuantity(InventoryResourceType.Scrap) >= WeaponGenerator.GetWeaponClassWithType(type).AmmoCost)
        {
            _canAfford.Add(button);
            return;
        }
        _cantAfford.Add(button);
    }

    private void UpdateHaveAmounts()
    {
        Helper.FindChildWithName<EnhancedText>(PistolButton.gameObject, "Right Text").Text(NumberOfMagazinesOfType(WeaponType.Pistol));
        Helper.FindChildWithName<EnhancedText>(RifleButton.gameObject, "Right Text").Text(NumberOfMagazinesOfType(WeaponType.Rifle));
        Helper.FindChildWithName<EnhancedText>(ShotgunButton.gameObject, "Right Text").Text(NumberOfMagazinesOfType(WeaponType.Shotgun));
        Helper.FindChildWithName<EnhancedText>(SmgButton.gameObject, "Right Text").Text(NumberOfMagazinesOfType(WeaponType.SMG));
        Helper.FindChildWithName<EnhancedText>(LmgButton.gameObject, "Right Text").Text(NumberOfMagazinesOfType(WeaponType.LMG));
    }

    private string NumberOfMagazinesOfType(WeaponType t)
    {
        int quantity = 0;
        switch (t)
        {
            case WeaponType.Pistol:
                quantity = (int) WorldState.HomeInventory().GetResourceQuantity(InventoryResourceType.PistolMag);
                break;
            case WeaponType.Rifle:
                quantity = (int) WorldState.HomeInventory().GetResourceQuantity(InventoryResourceType.RifleMag);
                break;
            case WeaponType.Shotgun:
                quantity = (int) WorldState.HomeInventory().GetResourceQuantity(InventoryResourceType.ShotgunMag);
                break;
            case WeaponType.SMG:
                quantity = (int) WorldState.HomeInventory().GetResourceQuantity(InventoryResourceType.SmgMag);
                break;
            case WeaponType.LMG:
                quantity = (int) WorldState.HomeInventory().GetResourceQuantity(InventoryResourceType.LmgMag);
                break;
        }
        return "have " + quantity;
    }

    private void CraftAmmo(InventoryResourceType type)
    {
        CraftAmmo craftState = (CraftAmmo) _player.States.GetState("Craft Ammo");
        craftState.SetAmmoType(type);
    }

    private void UpdateScrap()
    {
        ScrapLeft.Text(WorldState.HomeInventory().GetResourceQuantity(InventoryResourceType.Scrap) + " scrap left");
    }

    public static UICreateAmmoController Instance()
    {
        return _instance;
    }
}