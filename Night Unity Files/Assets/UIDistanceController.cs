using Game.Combat;
using SamsHelper;
using TMPro;
using UnityEngine;

public class UIDistanceController : MonoBehaviour
{
	private DetailedEnemyCombat _enemy;
	private TextMeshProUGUI _text;
	public bool InSight;

	private void Awake()
	{
		_text = GetComponent<TextMeshProUGUI>();
	}

	public void SetEnemy(DetailedEnemyCombat enemy)
	{
		_enemy = enemy;
	}

	private void SetInSight(bool inSight)
	{
		if (inSight == InSight) return;
		InSight = inSight;
		if (inSight)
		{
			_enemy.UiAimController.Show();
		}
		else
		{
			_enemy.UiAimController.Hide();
		}
	}
	
	public void Update ()
	{
		if (_enemy == null) return;
		string directionText = "Behind";
		bool inSight = false;
		if (_enemy._aheadOfPlayer && CombatManager.Player.FacingDirection == Direction.Right)
		{
			directionText = Helper.Round(_enemy.DistanceToPlayer) + "m";
			inSight = true;
		}

		else if (!_enemy._aheadOfPlayer && CombatManager.Player.FacingDirection == Direction.Left)
		{
			directionText = -Helper.Round(_enemy.DistanceToPlayer) + "m";
			inSight = true;
		}

		SetInSight(inSight);

		_text.text = directionText;
	}
}
