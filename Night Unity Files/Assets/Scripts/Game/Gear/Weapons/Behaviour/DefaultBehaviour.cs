using Game.Combat.Misc;

namespace Game.Gear.Weapons
{
    public class DefaultBehaviour : BaseWeaponBehaviour
    {
        private bool _firing;
        private float _accuracyModifier;

        public override void StartFiring(CharacterCombat origin)
        {
            base.StartFiring(origin);
            Fire(origin);
        }
    }
}