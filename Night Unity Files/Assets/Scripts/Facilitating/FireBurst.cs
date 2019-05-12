using UnityEngine;

namespace Facilitating
{
	public class FireBurst : MonoBehaviour
	{
		private ParticleSystem _particleSystem;
		private float          _targetTime;
		public  int            BurstMinSize = 100, BurstMaxSize = 150;
		public  float          MinBurstTime = 3,   MaxBurstTime = 5;

		public void Start()
		{
			_particleSystem = GetComponent<ParticleSystem>();
		}

		public void Update()
		{
			_targetTime -= Time.deltaTime;
			if (!(_targetTime <= 0)) return;
			_targetTime = Random.Range(MinBurstTime, MaxBurstTime);

			float   xPosition       = Random.Range(-8f, 8f);
			Vector3 currentPosition = transform.position;
			currentPosition.x  = xPosition;
			transform.position = currentPosition;

			int noToEmit = Random.Range(BurstMinSize, BurstMaxSize);
			_particleSystem.Emit(noToEmit);
		}
	}
}