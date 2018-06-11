using Game.Combat.Misc;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.Libraries;
using Random = UnityEngine.Random;

namespace Game.Gear.Weapons
{
    public class RandomFire : BaseWeaponBehaviour
    {
        public override void StartFiring(CharacterCombat origin)
        {
            base.StartFiring(origin);
            Fire(origin);
            float fireInterval = 1f / Weapon.GetAttributeValue(AttributeType.FireRate);
            fireInterval *= Random.Range(0.5f, 1.5f);
            TimeToNextFire = Helper.TimeInSeconds() + fireInterval;
        }
    }
}