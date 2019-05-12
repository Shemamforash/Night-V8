using System.Collections.Generic;
using Game.Combat.Generation;
using SamsHelper;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;

public class UiCombatRingBoundDrawer : MonoBehaviour
{
	private static          GameObject              _ringPrefab;
	private static          UiCombatRingBoundDrawer _instance;
	private static readonly List<GameObject>        _rings = new List<GameObject>();

	public void Awake()
	{
		_instance = this;
	}

	public void OnDestroy()
	{
		_instance = null;
	}

	public static void Draw(bool drawInner)
	{
		_rings.ForEach(Destroy);
		_rings.Clear();
		if (drawInner) CreateRing(WorldGrid.CombatMovementDistance / 2f, 0.02f, Color.white);
		CreateRing(WorldGrid.CombatAreaWidth / 2f, 0.01f, UiAppearanceController.FadedColour);
	}

	private static void CreateRing(float radius, float width, Color colour)
	{
		if (_ringPrefab == null) _ringPrefab = Resources.Load<GameObject>("Prefabs/Map/Map Ring");
		GameObject ring                      = Instantiate(_ringPrefab, _instance.transform, true);
		_rings.Add(ring);
		ring.transform.position   = Vector2.zero;
		ring.transform.localScale = Vector2.one;
		RingDrawer rd = ring.GetComponent<RingDrawer>();
		rd.SetLineWidth(width);
		rd.SetColor(colour);
		rd.SetRadius(radius);
	}
}