using Game.Combat.Enemies.Nightmares.EnemyAttackBehaviours;
using Game.Combat.Generation;
using UnityEngine;

namespace Game.Combat.Enemies.Nightmares
{
    public class Shadow : EnemyBehaviour
    {
        private float _stealHealthTimer;
        private const float StealHealthTimerMax = 5f;
        private Cell layMineTarget;
        private bool _layingMines;
        private float _layMineTimer;
        private const float LayMineTimerMax = 0.5f;
        private GameObject _minePrefab;


        public void Start()
        {
            gameObject.AddComponent<ErraticDash>();
//            gameObject.AddComponent<Slice>().Initialise(4, 2);
            gameObject.AddComponent<Push>().Initialise(4, 2);
        }
        
//        public override void ChooseNextAction()
//        {
//            if (_layingMines) return;
//            layMineTarget = PathingGrid.GetCellNearMe(PlayerCombat.Instance.CurrentCell(), 4f);
//            Reposition(layMineTarget, () =>
//            {
//                _layingMines = true;
//                layMineTarget = PathingGrid.GetCellNearMe(CurrentCell(), 5);
//                Reposition(layMineTarget, () => { _layingMines = false; });
//            });

//        }


        public override void Update()
        {
            base.Update();
//            if (!_layingMines) return;
//            _layMineTimer += Time.deltaTime;
//            if (_layMineTimer < LayMineTimerMax) return;
//            _layMineTimer = 0;
//            LayMine();
        }

        private void LayMine()
        {
            if (_minePrefab == null) _minePrefab = Resources.Load<GameObject>("Prefabs/Combat/Mine");
            GameObject mine = Instantiate(_minePrefab);
            mine.transform.position = transform.position;
            mine.transform.localScale = Vector3.one;
            mine.transform.rotation = Quaternion.Euler(new Vector3(0, 0, Random.Range(0, 360)));
        }
    }
}