using System;
using Game.Characters;
using Game.Characters.Player;
using Game.World.Region;
using SamsHelper;
using SamsHelper.ReactiveUI.Elements;
using SamsHelper.ReactiveUI.MenuSystem;
using TMPro;
using UnityEngine;

namespace Facilitating.UIControllers
{
	public class UIRegionItem : MonoBehaviour
	{
		private TextMeshProUGUI _leftText, _rightText, _centreText;
		private RectTransform _bookendContainer;
		private bool _updateBookends;

		public void Awake ()
		{
			_leftText = Helper.FindChildWithName<TextMeshProUGUI>(gameObject, "Left Text");
			_rightText = Helper.FindChildWithName<TextMeshProUGUI>(gameObject, "Right Text");
			_centreText = Helper.FindChildWithName<TextMeshProUGUI>(gameObject, "Centre Text");
			_bookendContainer = Helper.FindChildWithName<RectTransform>(gameObject, "Long Bookends");
		}

		public void SetText(string left, string centre, string right)
		{
			_leftText.text = left;
			_centreText.text = centre;
			_rightText.text = right;
			_updateBookends = true;
		}

		public void Update()
		{
			if (!_updateBookends) return;
			_bookendContainer.offsetMin = new Vector2(-(_leftText.GetComponent<RectTransform>().rect.width + 16), 0);
			_bookendContainer.offsetMax = new Vector2(_rightText.GetComponent<RectTransform>().rect.width + 16, 0);
			_updateBookends = false;
		}

		public void SetRegion(Region connection, Player player)
		{
			GetComponent<EnhancedButton>().AddOnClick(() =>
			{
				//Travel to unknown region
				player.TravelAction.TravelTo(connection);
				MenuStateMachine.GoToInitialMenu();
			});
		}
	}
}
