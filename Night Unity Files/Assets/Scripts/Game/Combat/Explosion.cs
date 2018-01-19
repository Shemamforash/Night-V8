using System.Collections.Generic;
using Game.Characters;
using UnityEngine;

namespace Game.Combat
{
    public class Explosion
    {
        private int _damage;
        private float _radius;
        private float _position;

        private float _knockbackDistance;
        private bool _bleed, _burn, _sick;

        public Explosion(float position, float radius, int damage)
        {
            _position = position;
            _radius = radius;
            _damage = damage;
        }

        public static void CreateAndDetonate(float position, float radius, int damage)
        {
            new Explosion(position, radius, damage).Fire();
        }

        public void Fire()
        {
            List<Character> _charactersInRange = CombatManager.GetCharactersInRange(_position, _radius);
            foreach (Character c in _charactersInRange)
            {
                float distance = CombatManager.DistanceBetween(_position, c);
                float normalisedDistance = distance / _radius;
                int damage = (int) (_damage * normalisedDistance);
                c.OnHit(damage, false);
                c.Knockback(_knockbackDistance * (1 - normalisedDistance));
                if (_bleed) c.AddBleedStack();
                if (_burn) c.AddBurnStack();
                if (_sick) c.AddSicknessStack();
            }
        }

        public void SetKnockbackDistance(float distance)
        {
            _knockbackDistance = distance;
        }

        public void SetBurning()
        {
            _burn = true;
        }

        public void SetBleeding()
        {
            _bleed = true;
        }

        public void SetSickness()
        {
            _sick = true;
        }
    }
}