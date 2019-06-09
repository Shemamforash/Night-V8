using Game.Characters;
using Extensions;
using SamsHelper.ReactiveUI.MenuSystem;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.Global
{
	public class WorldView : Menu
	{
		private static string          _environmentString, _weatherString, _timeString;
		private static TextMeshProUGUI _environmentText;
		private        Selectable      _lastSelectedButton;

		public static RectTransform GetEnvironmentRect() => _environmentText.GetComponent<RectTransform>();

		public static void SetEnvironmentText(string text)
		{
			_environmentString = text;
		}

		public static void SetWeatherText(string text)
		{
			_weatherString = text;
		}

		public static void MyUpdate(int hours)
		{
			_timeString = TimeToName(hours);
			UpdateDescription();
		}

		private static void UpdateDescription()
		{
			int    remainingTemples = WorldState.GetRemainingTemples();
			string templeString;
			switch (remainingTemples)
			{
				case 0:
					templeString = "No temples remain uncleansed, the gate is open.";
					break;
				case 1:
					templeString = "Only one temple remains uncleansed.";
					break;
				default:
					templeString = remainingTemples + " temples remain uncleansed";
					break;
			}

			_environmentText.text = _timeString + " in " + _environmentString + ". " + _weatherString + ". " + templeString;
		}

		protected override void Awake()
		{
			base.Awake();
			PauseOnOpen      = false;
			_environmentText = gameObject.FindChildWithName<TextMeshProUGUI>("Environment");
		}

		public override void Enter()
		{
			base.Enter();
			Player selectedCharacter = CharacterManager.SelectedCharacter;
			CharacterManager.SelectCharacter(selectedCharacter);
			if (_lastSelectedButton != null) _lastSelectedButton.Select();
		}

		public override void Exit()
		{
			base.Exit();
			GameObject selectedObject = EventSystem.current.currentSelectedGameObject;
			if (selectedObject == null) return;
			_lastSelectedButton = selectedObject.GetComponent<Selectable>();
		}

		public static string TimeToName(int hours)
		{
			if (hours >= 3 && hours <= 5) return "Predawn";
			if (hours >= 5 && hours <= 7) return "Dawn";
			if (hours > 7  && hours <= 9) return "Early Morning";
			if (hours > 9  && hours <= 11) return "Morning";
			if (hours > 11 && hours <= 13) return "Noon";
			if (hours > 13 && hours <= 15) return "Early Afternoon";
			if (hours > 15 && hours <= 17) return "Afternoon";
			if (hours > 17 && hours <= 19) return "Dusk";
			if (hours > 19 && hours < 21) return "Evening";
			if (hours > 23 || hours < 1) return "Midnight";
			return "Night";
		}
	}
}