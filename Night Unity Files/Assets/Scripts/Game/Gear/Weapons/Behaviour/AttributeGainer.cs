using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.Libraries;

namespace Game.Gear.Weapons
{
    public class AttributeGainer : Spoolup
    {
        private float _timeAtFirstFire;
        private const float TimeToMaxAccuracy = 2f;
        private bool _firing;
        private float _modifier;
        
        public override void StartFiring()
        {
            base.StartFiring();
            if (!ReadyToFire()) return;
            if (_firing == false) _timeAtFirstFire = Helper.TimeInSeconds();
            _firing = true;
            float difference = Helper.TimeInSeconds() - _timeAtFirstFire;
            float normalisedDifference = difference > TimeToMaxAccuracy ? 1 : difference / TimeToMaxAccuracy;
            _modifier = 1 + normalisedDifference;
            TimeToNextFire = Helper.TimeInSeconds() + 1f / (Weapon.GetAttributeValue(AttributeType.FireRate) * _modifier);
        }

        public override void StopFiring()
        {
            base.StopFiring();
            _timeAtFirstFire = 0f;
        }
    }
}