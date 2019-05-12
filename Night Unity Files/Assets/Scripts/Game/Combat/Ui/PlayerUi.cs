using System.Collections.Generic;
using Game.Combat.Generation;
using Game.Combat.Misc;
using Game.Combat.Player;
using Extensions;
using SamsHelper.Input;
using TMPro;
using UnityEngine;

namespace Game.Combat.Ui
{
	public class PlayerUi : CharacterUi
	{
		private TextMeshProUGUI _reloadKeyText, _compassKeyText;

		public void Start()
		{
			_reloadKeyText  = gameObject.FindChildWithName("Reload Key").FindChildWithName<TextMeshProUGUI>("Text");
			_compassKeyText = gameObject.FindChildWithName("Compass Key").FindChildWithName<TextMeshProUGUI>("Text");
			ControlTypeChangeListener controlTypeChangeListener = GetComponent<ControlTypeChangeListener>();
			controlTypeChangeListener.SetOnControllerInputChange(UpdateText);
		}

		private void UpdateText()
		{
			_reloadKeyText.SetText(InputHandler.GetBindingForKey(InputAxis.Reload));
			_compassKeyText.SetText(InputHandler.GetBindingForKey(InputAxis.Compass));
		}

		public void Update()
		{
			if (PlayerCombat.Instance == null) return;
			List<CanTakeDamage> chars            = CombatManager.Instance().GetEnemiesInRange(PlayerCombat.Position(), 5f);
			Vector2             playerDir        = PlayerCombat.Instance.transform.up;
			float               nearestAngle     = 360;
			CanTakeDamage       nearestCharacter = null;
			chars.ForEach(c =>
			{
				Vector2 enemyDir   = c.transform.position - PlayerCombat.Position();
				float   enemyAngle = Vector2.Angle(playerDir, enemyDir);
				if (enemyAngle > 10) return;
				if (enemyAngle > nearestAngle) return;
				nearestAngle     = enemyAngle;
				nearestCharacter = c;
			});
			PlayerCombat.Instance.SetTarget(nearestCharacter);
		}

		protected override void LateUpdate()
		{
			Character = PlayerCombat.Instance;
			base.LateUpdate();
		}
	}
}