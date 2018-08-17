using Game.Combat.Player;
using Game.Combat.Ui;
using Game.Global;
using SamsHelper.Input;
using UnityEngine;

public class RadianceController : MonoBehaviour, IInputListener
{
	private static RadianceController _instance;
	private static bool _active;
	private static GameObject _stonePrefab;

	public void Awake()
	{
		_instance = this;
	}
	
	public static void Activate()
	{
		if (_active) return;
		if (!PlayerCombat.Instance.Player.TravelAction.InClaimedRegion())
		{
			int radianceAvailable = WorldState.HomeInventory().GetResourceQuantity("Radiance");
			if (radianceAvailable == 0) return;
			PlayerUi.SetEventText("Claim region [T] (" + radianceAvailable + " radiance left)");
			InputHandler.RegisterInputListener(_instance);
		}
		_active = true;
	}

	public void OnInputDown(InputAxis axis, bool isHeld, float direction = 0)
	{
		if (axis != InputAxis.TakeItem) return;
		PlayerCombat.Instance.Player.TravelAction.GetCurrentNode().Claim();
		InputHandler.UnregisterInputListener(this);
		if (_stonePrefab == null) _stonePrefab = Resources.Load<GameObject>("Prefabs/Combat/Effects/Radiance Stone");
		GameObject stoneObject = Instantiate(_stonePrefab);
		stoneObject.transform.position = transform.position;
		PlayerUi.FadeTextOut();
	}

	public void OnInputUp(InputAxis axis)
	{
	}

	public void OnDoubleTap(InputAxis axis, float direction)
	{
	}
}
