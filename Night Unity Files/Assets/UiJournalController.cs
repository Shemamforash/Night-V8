using System.Collections.Generic;
using Facilitating.UIControllers;
using SamsHelper.BaseGameFunctionality.Basic;

public class UiJournalController : UiGearMenuTemplate
{
	public override bool GearIsAvailable()
	{
		return false;
	}

	public override void SelectGearItem(MyGameObject item, UiGearMenuController.GearUi gearUi)
	{
	}

	public override List<MyGameObject> GetAvailableGear()
	{
		return null;
	}

	public override void Equip(int selectedGear)
	{
	}
}
