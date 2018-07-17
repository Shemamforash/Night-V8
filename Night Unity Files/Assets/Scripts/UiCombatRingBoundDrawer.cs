using Game.Combat.Generation;
using SamsHelper;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;

public class UiCombatRingBoundDrawer : MonoBehaviour {
	public void Awake()
	{
		CreateRing(PathingGrid.CombatMovementDistance / 2f, 0.02f, Color.white);
		CreateRing(PathingGrid.CombatAreaWidth / 2f, 0.01f, UiAppearanceController.FadedColour);
	}
	
	private void CreateRing(float radius, float width, Color colour)
	{
		GameObject ring = new GameObject();
		ring.transform.SetParent(transform);
		ring.transform.position = Vector2.zero;
		ring.transform.localScale = Vector2.one;
		RingDrawer rd = ring.AddComponent<RingDrawer>();
		rd.SetLineWidth(width);
		rd.SetColor(colour);
		rd.DrawCircle(radius);
	}
}
