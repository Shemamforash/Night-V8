using System.Collections.Generic;
using SamsHelper.ReactiveUI.InventoryUI;
using UnityEngine;

public class UIGrenadeController : MonoBehaviour {
	public static readonly List<Grenade> Grenades = new List<Grenade>();
	private static readonly List<Grenade> GrenadesToRemove = new List<Grenade>();
	private static UIGrenadeController _instance;

	private void Awake()
	{
		_instance = this;
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
		Destroy(g.GrenadeView.GetGameObject());
	}
	
	public static void AddGrenade(Grenade g)
	{
		Grenades.Add(g);
		g.CreateUi(_instance.transform);
	}
}
