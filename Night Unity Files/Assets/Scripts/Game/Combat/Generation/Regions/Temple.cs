using UnityEngine;

namespace Game.Combat.Generation
{
	public class Temple : RegionGenerator
	{
		protected override void Generate()
		{
		}

		protected override void PlaceItems()
		{
		}

		protected override void GenerateObjects()
		{
			GameObject temple = Instantiate(Resources.Load<GameObject>("Prefabs/Combat/Buildings/Temple"));
			temple.transform.position = Vector2.zero;
		}
	}
}