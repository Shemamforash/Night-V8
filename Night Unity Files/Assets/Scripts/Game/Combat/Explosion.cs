using System.Collections.Generic;
using Game.Characters;
using UnityEngine;

namespace Game.Combat
{
    public class Explosion
    {
        private readonly int _damage;
        private readonly float _radius;
        private readonly float _position;

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
            List<Character> charactersInRange = CombatManager.GetCharactersInRange(_position, _radius);
            foreach (Character c in charactersInRange)
            {
                c.OnHit(_damage);
                c.Knockback(_knockbackDistance);
                if (_bleed) c.Bleeding.AddStack();
                if (_burn) c.Burn.AddStack();
                if (_sick) c.Sick.AddStack();
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