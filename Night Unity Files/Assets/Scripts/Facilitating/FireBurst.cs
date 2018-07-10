using UnityEngine;

namespace Facilitating
{
    public class FireBurst : MonoBehaviour
    {
        private ParticleSystem _particleSystem;
        private float _targetTime;
        public int BurstMinSize = 50, BurstMaxSize = 150;
        public float MinBurstTime = 3, MaxBurstTime = 7;

        // Use this for initialization
        public void Start()
        {
            _particleSystem = GetComponent<ParticleSystem>();
        }

        // Update is called once per frame
        public void Update()
        {
//            if (!(Campfire.Intensity() > 0)) return;
//            _targetTime -= Time.deltaTime;
//            if (!(_targetTime <= 0)) return;
//            _targetTime = Random.Range(MinBurstTime, MaxBurstTime);
//
//            float xPosition = Random.Range(-10f, 10f);
//            Vector3 currentPosition = transform.position;
//            currentPosition.x = xPosition;
//            transform.position = currentPosition;
//
//            int burstMin = (int) (BurstMinSize * Campfire.Intensity());
//            int burstMax = (int) (BurstMaxSize * Campfire.Intensity());
//
//            int noToEmit = Random.Range(burstMin, burstMax);
//            _particleSystem.Emit(noToEmit);
        }
    }
}