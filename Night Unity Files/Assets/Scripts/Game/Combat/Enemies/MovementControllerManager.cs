using System.Collections.Generic;
using UnityEngine;

namespace Game.Combat.Enemies
{
	public class MovementControllerManager : MonoBehaviour
	{
		private static readonly List<MovementController> _movementControllers = new List<MovementController>();

		private void FixedUpdate()
		{
			_movementControllers.ForEach(m => m.MyFixedUpdate());
		}

		private void OnDestroy()
		{
			_movementControllers.Clear();
		}

		public static void RegisterMovementController(MovementController controller) => _movementControllers.Add(controller);

		public static void UnregisterMovementController(MovementController controller) => _movementControllers.Remove(controller);
	}
}