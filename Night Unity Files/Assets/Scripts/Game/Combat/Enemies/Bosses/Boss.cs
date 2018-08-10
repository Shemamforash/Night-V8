﻿using System.Collections.Generic;
using UnityEngine;

namespace Game.Combat.Enemies.Bosses
{
    public abstract class Boss : MonoBehaviour
    {
        protected readonly List<BossSectionHealthController> Sections = new List<BossSectionHealthController>();
        protected Rigidbody2D RigidBody;

        public virtual void Awake()
        {
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

        protected virtual void Kill()
        {
            Destroy(gameObject);
        }

        protected int SectionCount()
        {
            return Sections.Count;
        }
    }
}