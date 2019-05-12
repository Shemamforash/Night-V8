namespace Game.Gear.Weapons
{
	public class DefaultBehaviour : BaseWeaponBehaviour
	{
		private float _accuracyModifier;
		private bool  _firing;

		public override void StartFiring()
		{
			base.StartFiring();
			Fire();
		}
	}
}