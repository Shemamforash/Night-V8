using Game.Combat.Misc;

namespace Game.Gear.Weapons
{
    public class DoubleFireInstant : BaseWeaponBehaviour
    {
        public override void StartFiring(CharacterCombat origin)
        {
            base.StartFiring(origin);
            Fire(origin);
            Fire(origin);
        }
    }
}