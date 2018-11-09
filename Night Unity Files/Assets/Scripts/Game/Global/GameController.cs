using Facilitating.Persistence;
using Game.Characters.CharacterActions;
using Game.Global;
using SamsHelper.Input;
using SamsHelper.ReactiveUI.MenuSystem;
using UnityEngine;

public class GameController : MonoBehaviour {
	private bool _starting;

	public void Awake()
	{
		_starting = false;
		Cursor.visible = false;
	}

	public void StartGame(bool newGame, Travel travel = null)
	{
		_starting = true;
		InputHandler.SetCurrentListener(null);
		if (newGame) StoryController.ShowText(JournalEntry.GetStoryText(1), false);
		else {
			if (travel != null) travel.Enter();
			else SceneChanger.GoToGameScene();
			MenuStateMachine.ShowMenu("Loading Screen");
		}
	}

	public void ContinueGame()
	{
		if (_starting) return;
		Travel travel = SaveController.LoadGame();
		StartGame(false, travel);
	}

	public void QuitToDesktop()
	{
		Application.Quit();
	}

	private void ClearSaveAndLoad()
	{
		if (_starting) return;
		_starting = true;
		SaveController.ClearSave();
		WorldState.ResetWorld();
		SaveController.SaveGame();
		StartGame(true);
	}

	public void EnableTutorial()
	{
		ClearSaveAndLoad();
		TutorialManager.SetTutorialActive(true);
	}

	public void DisableTutorial()
	{
		ClearSaveAndLoad();
		TutorialManager.SetTutorialActive(false);
	}
}
