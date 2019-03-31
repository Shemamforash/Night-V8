using System.Collections.Generic;
using System.Linq;
using System.Xml;
using DG.Tweening;
using Facilitating.Persistence;
using Game.Combat.Generation;
using Game.Exploration.Regions;
using Game.Global;
using Game.Global.Tutorial;
using SamsHelper.Input;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    private static readonly Dictionary<int, List<TutorialPart>> _tutorialParts = new Dictionary<int, List<TutorialPart>>();
    private static TutorialPart _currentTutorialPart;
    private static bool _loaded;
    private static EnhancedText _titleText, _contentText;
    private static TutorialOverlayController _overlayController;
    private static CloseButtonController _closeButton;
    private static bool _tutorialActive;
    private static CanvasGroup _tutorialCanvas;
    private static IInputListener _lastListener;
    private static bool _showingTutorial;
    private static bool _alreadyPaused;
    private static bool _alreadyHidden;

    private static List<TutorialOverlay> _overlays;
    private static bool _hideResouces, _finishedIntroTutorial;
    private static GameObject _lastSelectedObject;

    public void Awake()
    {
        GameObject top = gameObject.FindChildWithName("Instructions Top");
        _titleText = top.FindChildWithName<EnhancedText>("Title");
        _contentText = top.FindChildWithName<EnhancedText>("Text");
        _overlayController = gameObject.GetComponent<TutorialOverlayController>();
        _closeButton = top.FindChildWithName<CloseButtonController>("Close Button");
        _tutorialCanvas = GetComponent<CanvasGroup>();
        _tutorialCanvas.alpha = 0f;
    }

    private void Start()
    {
        _closeButton.UseAcceptInput();
    }

    public static bool TryOpenTutorial(int tutorialPart, TutorialOverlay overlay, bool hideResources = true)
    {
        return TryOpenTutorial(tutorialPart, new List<TutorialOverlay> {overlay}, hideResources);
    }

    public static bool TryOpenTutorial(int tutorialPart, List<TutorialOverlay> overlays, bool hideResources = true)
    {
        if (!_tutorialActive || _showingTutorial) return false;
        if (PauseMenuController.IsOpen()) PauseMenuController.ToggleOpen();
        _overlays = overlays;
        _hideResouces = hideResources;
        ReadTutorialParts();
        _currentTutorialPart = _tutorialParts[tutorialPart].FirstOrDefault(p => !p.IsComplete());
        if (_currentTutorialPart == null) return false;
        EnableButton();
        ShowTutorialPart();
        Enter();
        return true;
    }

    private static void EnableButton()
    {
        _lastSelectedObject = EventSystem.current.currentSelectedGameObject;
        EventSystem.current.SetSelectedGameObject(_closeButton.Button().gameObject);
        SetCurrentSelectableActive(false);
        _lastListener = InputHandler.GetCurrentListener();
        InputHandler.InterruptListeners(true);
        InputHandler.SetCurrentListener(_closeButton);
        _closeButton.SetOnClick(ShowTutorialPart);
    }

    private static void SetCurrentSelectableActive(bool active)
    {
        if (_lastSelectedObject == null) return;
        _lastSelectedObject.GetComponent<Selectable>().enabled = active;
    }

    public static Dictionary<int, List<TutorialPart>> GetTutorialParts()
    {
        ReadTutorialParts();
        return _tutorialParts;
    }

    private static void ShowTutorialPart()
    {
        string title = _currentTutorialPart.Title;
        string content = _currentTutorialPart.Content;
        _titleText.SetText(title);
        _contentText.SetText(content);
        _overlayController.SetTutorialArea(_overlays[_currentTutorialPart.PartNumber - 1]);
        _currentTutorialPart.MarkComplete();
        if (_currentTutorialPart.NextPart() == null)
        {
            _closeButton.SetOnClick(Close);
        }
        else _currentTutorialPart = _currentTutorialPart.NextPart();
    }

    private static void Enter()
    {
        _tutorialCanvas.blocksRaycasts = true;
        _tutorialCanvas.interactable = true;
        _alreadyHidden = ResourcesUiController.Hidden();
        if (!_alreadyHidden && _hideResouces) ResourcesUiController.Hide();
        _showingTutorial = true;
        DOTween.defaultTimeScaleIndependent = true;
        bool combatPaused = CombatManager.Instance() != null && !CombatManager.Instance().IsCombatActive();
        bool gamePaused = CombatManager.Instance() == null && WorldState.Paused();
        _alreadyPaused = combatPaused || gamePaused;
        if (!_alreadyPaused) WorldState.Pause();
        _tutorialCanvas.DOFade(1f, 0.5f);
    }

    private static void Close()
    {
        if (!_alreadyHidden) ResourcesUiController.Show();
        _tutorialCanvas.blocksRaycasts = false;
        _tutorialCanvas.interactable = false;
        _closeButton.SetOnClick(null);
        _closeButton.Disable();
        Sequence sequence = DOTween.Sequence();
        sequence.Append(_tutorialCanvas.DOFade(0f, 0.5f));
        sequence.AppendCallback(() =>
        {
            DOTween.defaultTimeScaleIndependent = false;
            if (!_alreadyPaused) WorldState.Resume();
            _tutorialCanvas.blocksRaycasts = false;
            _showingTutorial = false;
        });
        SetCurrentSelectableActive(true);
        InputHandler.SetCurrentListener(_lastListener);
        InputHandler.InterruptListeners(false);
    }

    private void Update()
    {
        if (!IsTutorialVisible()) return;
        if (EventSystem.current.currentSelectedGameObject == _closeButton.Button().gameObject) return;
        _closeButton.Button().Select();
    }
    
    public static void Load(XmlNode root)
    {
        ReadTutorialParts(true);
        root = root.SelectSingleNode("Tutorial");
        _tutorialActive = root.BoolFromNode("TutorialActive");
        _finishedIntroTutorial = root.BoolFromNode("FinishedIntro");
        if (!_tutorialActive) return;
        foreach (XmlNode sectionNode in root.SelectSingleNode("TutorialParts").ChildNodes)
        {
            int sectionNumber = sectionNode.IntFromNode("SectionNumber");
            foreach (XmlNode partNode in sectionNode.SelectNodes("Part"))
            {
                int partNumber = partNode.IntFromNode("PartNumber");
                bool completed = partNode.BoolFromNode("Completed");
                if (!completed) continue;
                _tutorialParts[sectionNumber][partNumber - 1].MarkComplete();
            }
        }
    }

    public static void Save(XmlNode root)
    {
        root = root.CreateChild("Tutorial");
        root.CreateChild("TutorialActive", _tutorialActive);
        root.CreateChild("FinishedIntro", _finishedIntroTutorial);
        root = root.CreateChild("TutorialParts");
        foreach (int section in _tutorialParts.Keys)
        {
            XmlNode tutorialSection = root.CreateChild("Section");
            tutorialSection.CreateChild("SectionNumber", section);
            _tutorialParts[section].ForEach(p => p.SaveTutorialPart(tutorialSection));
        }
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

    public static bool IsTutorialVisible()
    {
        return _showingTutorial;
    }

    public static void SetTutorialActive(bool tutorialActive)
    {
        _tutorialActive = tutorialActive;
    }

    public static bool FinishedIntroTutorial() => _finishedIntroTutorial;

    public static bool Active()
    {
        return _tutorialActive;
    }

    public static void FinishIntroTutorial()
    {
        _finishedIntroTutorial = true;
    }
}