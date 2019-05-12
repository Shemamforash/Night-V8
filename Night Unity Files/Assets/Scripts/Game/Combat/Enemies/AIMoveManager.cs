using System.Collections.Generic;
using NUnit.Framework;

namespace Game.Combat.Enemies
{
	public static class AIMoveManager
	{
		private const           int                       BucketSize   = 5;
		private static readonly List<List<MoveBehaviour>> _moveBuckets = new List<List<MoveBehaviour>>();
		private static          int                       _currentUpdateBucket;

		public static void AddToBucket(MoveBehaviour moveBehaviour)
		{
			bool added = false;
			foreach (List<MoveBehaviour> bucket in _moveBuckets)
			{
				if (bucket.Count >= BucketSize) continue;
				bucket.Add(moveBehaviour);
				added = true;
				break;
			}

			if (added) return;
			List<MoveBehaviour> newBucket = new List<MoveBehaviour>();
			newBucket.Add(moveBehaviour);
			_moveBuckets.Add(newBucket);
		}

		public static void RemoveFromBucket(MoveBehaviour behaviour)
		{
			List<MoveBehaviour> bucket = _moveBuckets.Find(b => b.Contains(behaviour));
			Assert.IsNotNull(bucket);
			bucket.Remove(behaviour);
			if (bucket.Count != 0) return;
			_moveBuckets.Remove(bucket);
			if (_currentUpdateBucket >= _moveBuckets.Count) _currentUpdateBucket = _moveBuckets.Count - 1;
			if (_moveBuckets.Count   == 0) _currentUpdateBucket                  = 0;
		}

		public static void UpdateMoveBehaviours()
		{
			if (_moveBuckets.Count == 0) return;
			_moveBuckets[_currentUpdateBucket].ForEach(b => b.UpdatePath());
			++_currentUpdateBucket;
			if (_currentUpdateBucket < _moveBuckets.Count) return;
			_currentUpdateBucket = 0;
		}
	}
}