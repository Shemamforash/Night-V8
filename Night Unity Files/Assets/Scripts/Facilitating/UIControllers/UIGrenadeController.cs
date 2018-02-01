using System.Collections.Generic;
using SamsHelper.ReactiveUI.InventoryUI;
using UnityEngine;

public class UIGrenadeController : MonoBehaviour {
	private static readonly List<Grenade> Grenades = new List<Grenade>();
	private static readonly List<Grenade> GrenadesToRemove = new List<Grenade>();
	private static MenuList _grenadeList;

	public void Awake()
	{
		_grenadeList = GetComponent<MenuList>();
	}
	
	public void Update()
	{
		if (MeleeController.InMelee) return;
		Grenades.ForEach(g => { g.UpdateCombat(); });
		GrenadesToRemove.ForEach(g => Grenades.Remove(g));
		GrenadesToRemove.Clear();
	}
	
	public static void RemoveGrenade(Grenade g)
	{
		GrenadesToRemove.Add(g);
		_grenadeList.Remove(g.GrenadeView);
	}
	
	public static void AddGrenade(Grenade g)
	{
		Grenades.Add(g);
		_grenadeList.AddItem(g);
	}
}
