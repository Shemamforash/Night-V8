using System.Collections.Generic;
using Game.Combat.Generation;
using Game.Combat.Misc;
using Game.Global;
using UnityEngine;

namespace Game.Combat.Enemies.Bosses
{
    public class Boss : MonoBehaviour
    {
        protected readonly List<BossSectionHealthController> Sections = new List<BossSectionHealthController>();
        protected Rigidbody2D RigidBody;

        protected virtual void Awake()
        {
            gameObject.layer = 24;
            RigidBody = GetComponent<Rigidbody2D>();
        }

        public void RegisterSection(BossSectionHealthController section)
        {
            Sections.Add(section);
        }

        public virtual void UnregisterSection(BossSectionHealthController section)
        {
            Sections.Remove(section);
            if (Sections.Count == 0) Kill();
        }

        public void Kill()
        {
            Destroy(gameObject);
            CombatManager.ExitCombat(false);
            WorldState.TravelToNextEnvironment();
        }

        protected int SectionCount()
        {
            return Sections.Count;
        }
    }
}