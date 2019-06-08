using System.Collections.Generic;
using System.Linq;
using System.Xml;
using DG.Tweening;
using Facilitating.Persistence;
using Game.Combat.Generation;
using Game.Global;
using Game.Global.Tutorial;
using Extensions;
using SamsHelper.Input;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
	private static readonly Dictionary<int, List<TutorialPart>> _tutorialParts = new Dictionary<int, List<TutorialPart>>();
	private static          TutorialManager                     _instance;
	private static          bool                                _tutorialActive;
	private static          bool                                _loaded;
	private static          bool                                _finishedIntroTutorial;
	private                 bool                                _alreadyHidden;
	private                 bool                                _alreadyPaused;
	private                 CloseButtonController               _closeButton;

	private TutorialPart              _currentTutorialPart;
	private bool                      _hideResouces;
	private IInputListener            _lastListener;
	private GameObject                _lastSelectedObject;
	private TutorialOverlayController _overlayController;
	private List<TutorialOverlay>     _overlays;
	private bool                      _showingTutorial;
	private float                     _timeToWait;
	private EnhancedText              _titleText, _contentText;
	private CanvasGroup               _tutorialCanvas;

	public static TutorialManager Instance => _instance;

	public void Awake()
	{
		_instance = this;
		GameObject top = gameObject.FindChildWithName("Instructions Top");
		_titleText            = top.FindChildWithName<EnhancedText>("Title");
		_contentText          = top.FindChildWithName<EnhancedText>("Text");
		_overlayController    = gameObject.GetComponent<TutorialOverlayController>();
		_closeButton          = top.FindChildWithName<CloseButtonController>("Close Button");
		_tutorialCanvas       = GetComponent<CanvasGroup>();
		_tutorialCanvas.alpha = 0f;
	}

	private void OnDestroy()
	{
		_instance = null;
	}

	private void Start()
	{
		_closeButton.UseAcceptInput();
	}

	public bool TryOpenTutorial(int tutorialPart, TutorialOverlay overlay, bool hideResources = true) => TryOpenTutorial(tutorialPart, new List<TutorialOverlay> {overlay}, hideResources);

	public bool TryOpenTutorial(int tutorialPart, List<TutorialOverlay> overlays, bool hideResources = true)
	{
		if (!_tutorialActive || _showingTutorial) return false;
		if (PauseMenuController.IsOpen()) PauseMenuController.ToggleOpen();
		_overlays     = overlays;
		_hideResouces = hideResources;
		ReadTutorialParts();
		_currentTutorialPart = _tutorialParts[tutorialPart].FirstOrDefault(p => !p.IsComplete());
		if (_currentTutorialPart == null) return false;
		EnableButton();
		ShowTutorialPart();
		Enter();
		return true;
	}

	private void EnableButton()
	{
		_lastSelectedObject = EventSystem.current.currentSelectedGameObject;
		if (_lastSelectedObject != _closeButton.Button().gameObject)
		{
			SetCurrentSelectableActive(false);
		}

		if (!EventSystem.current.alreadySelecting) EventSystem.current.SetSelectedGameObject(_closeButton.Button().gameObject);

		_lastListener = InputHandler.GetCurrentListener();
		InputHandler.InterruptListeners(true);
		InputHandler.SetCurrentListener(_closeButton);
		_timeToWait = 1f;
		if (_currentTutorialPart.NextPart() == null)
		{
			_closeButton.SetOnClick(Close);
		}
		else
		{
			_closeButton.SetOnClick(ShowTutorialPart);
		}
	}

	private void SetCurrentSelectableActive(bool active)
	{
		if (_lastSelectedObject == null) return;
		if (active) EventSystem.current.SetSelectedGameObject(_lastSelectedObject);
		_lastSelectedObject.GetComponent<Selectable>().enabled = active;
	}

	public static Dictionary<int, List<TutorialPart>> GetTutorialParts()
	{
		ReadTutorialParts();
		return _tutorialParts;
	}

	private void ShowTutorialPart()
	{
		string title   = _currentTutorialPart.Title;
		string content = _currentTutorialPart.Content;
		_titleText.SetText(title);
		_contentText.SetText(content);
		_overlayController.SetTutorialArea(_overlays[_currentTutorialPart.PartNumber - 1]);
		_currentTutorialPart.MarkComplete();
		_timeToWait = 1f;
		if (_currentTutorialPart.NextPart() == null)
		{
			_closeButton.SetOnClick(Close);
		}
		else
		{
			_currentTutorialPart = _currentTutorialPart.NextPart();
		}
	}

	private void Enter()
	{
		_tutorialCanvas.blocksRaycasts      = true;
		_tutorialCanvas.interactable        = true;
		_showingTutorial                    = true;
		DOTween.defaultTimeScaleIndependent = true;
		bool combatPaused = CombatManager.Instance() != null && !CombatManager.Instance().IsCombatActive();
		bool gamePaused   = CombatManager.Instance() == null && WorldState.Paused();
		_alreadyPaused = combatPaused || gamePaused;
		if (!_alreadyPaused) WorldState.Pause();
		_tutorialCanvas.DOFade(1f, 0.5f);
	}

	private void Close()
	{
		if (_timeToWait > 0f) return;
		_tutorialCanvas.blocksRaycasts = false;
		_tutorialCanvas.interactable   = false;
		_closeButton.SetOnClick(null);
		_closeButton.Disable();
		Sequence sequence = DOTween.Sequence();
		sequence.Append(_tutorialCanvas.DOFade(0f, 0.5f));
		sequence.AppendCallback(() =>
		{
			DOTween.defaultTimeScaleIndependent = false;
			if (!_alreadyPaused) WorldState.Resume();
			_tutorialCanvas.blocksRaycasts = false;
			_showingTutorial               = false;
			SetCurrentSelectableActive(true);
		});
		InputHandler.SetCurrentListener(_lastListener);
		InputHandler.InterruptListeners(false);
	}

	private void Update()
	{
		if (!IsTutorialVisible()) return;
		if (_timeToWait                                   > 0f) _timeToWait -= Time.unscaledDeltaTime;
		if (EventSystem.current.currentSelectedGameObject == _closeButton.Button().gameObject) return;
		_closeButton.Button().Select();
	}

	public static void Load(XmlNode root)
	{
		ReadTutorialParts(true);
		root                   = root.SelectSingleNode("Tutorial");
		_tutorialActive        = root.ParseBool("TutorialActive");
		_finishedIntroTutorial = root.ParseBool("FinishedIntro");
		if (!_tutorialActive) return;
		foreach (XmlNode sectionNode in root.SelectSingleNode("TutorialParts").ChildNodes)
		{
			int sectionNumber = sectionNode.ParseInt("SectionNumber");
			foreach (XmlNode partNode in sectionNode.SelectNodes("Part"))
			{
				int  partNumber = partNode.ParseInt("PartNumber");
				bool completed  = partNode.ParseBool("Completed");
				if (!completed) continue;
				if (!_tutorialParts.TryGetValue(sectionNumber, out List<TutorialPart> tutorialPartsList)) continue;
				int partIndex = partNumber - 1;
				if (tutorialPartsList.Count <= partIndex) continue;
				tutorialPartsList[partIndex].MarkComplete();
			}
		}
	}

	public static void Save(XmlNode root)
	{
		root = root.CreateChild("Tutorial");
		root.CreateChild("TutorialActive", _tutorialActive);
		root.CreateChild("FinishedIntro",  _finishedIntroTutorial);
		root = root.CreateChild("TutorialParts");
		foreach (int section in _tutorialParts.Keys)
		{
			XmlNode tutorialSection = root.CreateChild("Section");
			tutorialSection.CreateChild("SectionNumber", section);
			_tutorialParts[section].ForEach(p => p.SaveTutorialPart(tutorialSection));
		}
	}

	public static void ResetTutorial()
	{
		ReadTutorialParts(true);
	}

	private static void ReadTutorialParts(bool force = false)
	{
		if (!force && _loaded) return;
		_tutorialParts.Clear();
		XmlNode root = Helper.OpenRootNode("Tutorial", "Tutorial");
		foreach (XmlNode tutorialNode in root.SelectNodes("TutorialPart"))
		{
			TutorialPart part = new TutorialPart(tutorialNode);
			if (!_tutorialParts.ContainsKey(part.SectionNumber)) _tutorialParts.Add(part.SectionNumber, new List<TutorialPart>());
			_tutorialParts[part.SectionNumber].Add(part);
		}

		foreach (int section in _tutorialParts.Keys)
		{
			_tutorialParts[section].Sort((a, b) => a.PartNumber.CompareTo(b.PartNumber));
			List<TutorialPart> parts = _tutorialParts[section];
			for (int i = 0; i < parts.Count; ++i)
			{
				if (i + 1 == parts.Count) break;
				parts[i].SetNextPart(parts[i + 1]);
			}
		}

		_loaded = true;
	}

	public bool IsTutorialVisible() => _showingTutorial;

	public static void SetTutorialActive(bool tutorialActive)
	{
		_tutorialActive = tutorialActive;
	}

	public static bool FinishedIntroTutorial() => _finishedIntroTutorial;

	public static bool Active() => _tutorialActive;

	public static void FinishIntroTutorial()
	{
		_finishedIntroTutorial = true;
	}
}