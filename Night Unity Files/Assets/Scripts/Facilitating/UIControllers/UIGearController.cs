using System.Collections.Generic;
using Facilitating.MenuNavigation;
using Facilitating.UI.Elements;
using Game.Characters;
using Game.World;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.Characters;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.ReactiveUI.Elements;
using UIControllers;
using UnityEngine;

public class UIGearController : MonoBehaviour
{
    private EnhancedText _gearName, _quickDescription;
    public GearSubtype _gearType;

    public void Awake()
    {
        _gearName = Helper.FindChildWithName<EnhancedText>(gameObject, "Name");
        _quickDescription = Helper.FindChildWithName<EnhancedText>(gameObject, "Summary");
        GetComponent<EnhancedButton>().AddOnClick(() => OpenEquipMenu(CharacterManager.SelectedCharacter));
    }
    
    private void OpenEquipMenu(Character character)
    {
        List<MyGameObject> availableGear = new List<MyGameObject>();
        List<MyGameObject> allGear = new List<MyGameObject>();
        allGear.AddRange(character.Inventory().Contents());
        allGear.AddRange(WorldState.HomeInventory().Contents());
        foreach (MyGameObject item in allGear)
        {
            GearItem gear = item as GearItem;
            if (gear != null && gear.GetGearType() == _gearType && !gear.Equipped)
            {
                availableGear.Add(gear);
            }
        }
        UIGearEquipController.DisplayGear(character, availableGear);
    }

    public void SetGearItem(GearItem item)
    {
        if (item != null)
        {
            _gearName.Text(item.Name);
            _quickDescription.Text(item.GetSummary());
        }
        else
        {
            _gearName.Text("- Not Equipped -");
            _quickDescription.Text("--");
        }
    }
}
