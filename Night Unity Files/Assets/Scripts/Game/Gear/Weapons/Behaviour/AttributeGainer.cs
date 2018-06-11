using Game.Combat.Misc;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.Libraries;

namespace Game.Gear.Weapons
{
    public class AttributeGainer : BaseWeaponBehaviour
    {
        private float _timeAtFirstFire;
        private const float TimeToMaxAccuracy = 2f;
        private bool _firing;
        private float _modifier;
        
        public override void StartFiring(CharacterCombat origin)
        {
            base.StartFiring(origin);
            if (_firing == false) _timeAtFirstFire = Helper.TimeInSeconds();
            _firing = true;
            float difference = Helper.TimeInSeconds() - _timeAtFirstFire;
            float normalisedDifference = difference > TimeToMaxAccuracy ? 1 : difference / TimeToMaxAccuracy;
            _modifier = 1 + normalisedDifference;
            Fire(origin);
            TimeToNextFire = Helper.TimeInSeconds() + 1f / (Weapon.GetAttributeValue(AttributeType.FireRate) * _modifier);
        }

        public override void EndFiring()
        {
            _timeAtFirstFire = 0f;
        }
    }
}