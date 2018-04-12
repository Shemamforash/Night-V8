using SamsHelper.ReactiveUI.MenuSystem;

public class UiAreaInventoryController : Menu
{
	private static ContainerController _lastNearestContainer;

	public override void Awake()
	{
		base.Awake();
		_lastNearestContainer = null;
	}
	
	public static void SetNearestContainer (ContainerController nearestContainer)
	{
		if (nearestContainer != null)
		{
			if (nearestContainer != _lastNearestContainer)
			{
				MenuStateMachine.ShowMenu("Inventory");
			}
		}

		else if(_lastNearestContainer != null)
		{
			MenuStateMachine.ShowMenu("HUD");
		}

		_lastNearestContainer = nearestContainer;
	}
}
