using System.Collections.Generic;
using Game.Combat.Player;
using SamsHelper.BaseGameFunctionality.Basic;
using UnityEngine;

namespace Game.Combat.Misc
{
    public class ShotManager : MonoBehaviour
    {
        private static ObjectPool<Shot> _shotPool;

        public void Awake()
        {
            _shotPool = new ObjectPool<Shot>("Prefabs/Combat/Shots/Bullet", transform);
        }

        public static Shot Create(CharacterCombat origin)
        {
            Shot shot = _shotPool.Create();
            shot.gameObject.layer = origin is PlayerCombat ? 16 : 15;
            Vector3 direction = origin.Direction();
            shot.Initialise(origin, direction);
            return shot;
        }

        private void FixedUpdate()
        {
            Shots().ForEach(s => s.MyFixedUpdate());
        }

        public static List<Shot> Shots() => _shotPool.Active();

        public static void Return(Shot shot) => _shotPool.Return(shot);

        public static void Dispose(Shot shot) => _shotPool.Dispose(shot);
    }
}