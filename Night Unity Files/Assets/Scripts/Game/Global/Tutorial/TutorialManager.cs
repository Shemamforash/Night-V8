using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using DG.Tweening;
using Facilitating.Persistence;
using Game.Combat.Generation;
using Game.Combat.Player;
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

    private static bool _seenControlsGuide;
    private static List<TutorialOverlay> _overlays;
    private static bool _hideResouces;
    private static GraphicRaycaster _mainCanvasRaycaster;

    public void Awake()
    {
        GameObject top = gameObject.FindChildWithName("Instructions Top");
        _titleText = top.FindChildWithName<EnhancedText>("Title");
        _contentText = top.FindChildWithName<EnhancedText>("Text");
        _overlayController = gameObject.GetComponent<TutorialOverlayController>();
        _closeButton = top.FindChildWithName<CloseButtonController>("Close Button");
        _closeButton.UseFireInput();
        _tutorialCanvas = GetComponent<CanvasGroup>();
        _tutorialCanvas.alpha = 0f;
        _mainCanvasRaycaster = GameObject.Find("Canvas").GetComponent<GraphicRaycaster>();
    }

    public void Start()
    {
        StartCoroutine(ShowControlsTutorial());
    }

    public static bool TryOpenTutorial(int tutorialPart, List<TutorialOverlay> overlays, bool hideResources = true)
    {
        if (!_tutorialActive) return false;
        if (_showingTutorial) return false;
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
        _lastListener = InputHandler.GetCurrentListener();
        InputHandler.InterruptListeners(true);
        InputHandler.SetCurrentListener(_closeButton);
        _closeButton.SetCallback(ShowTutorialPart);
        _closeButton.SetOnClick(ShowTutorialPart);
        SetCurrentSelectableActive(false);
    }

    private static void SetCurrentSelectableActive(bool active)
    {
        GameObject selectedObject = EventSystem.current.currentSelectedGameObject;
        if (selectedObject == null) return;
        selectedObject.GetComponent<Selectable>().enabled = active;
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
            _closeButton.SetCallback(Close);
            _closeButton.SetOnClick(Close);
        }
        else _currentTutorialPart = _currentTutorialPart.NextPart();
    }

    private static void Enter()
    {
        _mainCanvasRaycaster.enabled = false;
        _tutorialCanvas.blocksRaycasts = true;
        _tutorialCanvas.interactable = true;
        _alreadyHidden = ResourcesUiController.Hidden();
        if (!_alreadyHidden && _hideResouces) ResourcesUiController.Hide();
        _showingTutorial = true;
        DOTween.defaultTimeScaleIndependent = true;
        _alreadyPaused = CombatManager.IsCombatActive() ? CombatManager.IsCombatPaused() : WorldState.Paused();
        _tutorialCanvas.blocksRaycasts = true;
        if (!_alreadyPaused)
        {
            WorldState.Pause();
            CombatManager.Pause();
        }

        _tutorialCanvas.DOFade(1f, 0.5f);
    }

    private static void Close()
    {
        if (!_alreadyHidden) ResourcesUiController.Show();
        _mainCanvasRaycaster.enabled = true;
        _tutorialCanvas.blocksRaycasts = false;
        _tutorialCanvas.interactable = false;
        _closeButton.SetCallback(null);
        Sequence sequence = DOTween.Sequence();
        sequence.Append(_tutorialCanvas.DOFade(0f, 0.5f));
        sequence.AppendCallback(() =>
        {
            DOTween.defaultTimeScaleIndependent = false;
            if (!_alreadyPaused)
            {
                WorldState.UnPause();
                CombatManager.Resume();
            }

            _tutorialCanvas.blocksRaycasts = false;
            _showingTutorial = false;
        });
        SetCurrentSelectableActive(true);
        InputHandler.SetCurrentListener(_lastListener);
        InputHandler.InterruptListeners(false);
    }

    public static void Load(XmlNode root)
    {
        ReadTutorialParts(true);
        root = root.SelectSingleNode("Tutorial");
        _tutorialActive = root.BoolFromNode("TutorialActive");
        _seenControlsGuide = root.BoolFromNode("SeenControls");
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
        root.CreateChild("SeenControls", _seenControlsGuide);
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

    public static bool SeenControlsGuide() => _seenControlsGuide;

    private IEnumerator ShowControlsTutorial()
    {
        if (!_tutorialActive || _seenControlsGuide || PlayerCombat.Instance == null) yield break;
        EventTextController.SetOverrideText("Move using [WASD]");
        while (Input.GetAxis("Horizontal") == 0 && Input.GetAxis("Vertical") == 0) yield return null;
        EventTextController.CloseOverrideText();
        yield return new WaitForSeconds(3);

        EventTextController.SetOverrideText("Rotate using [J] and [L]");
        while (Input.GetAxis("SwitchTab") == 0) yield return null;
        EventTextController.CloseOverrideText();
        yield return new WaitForSeconds(3);

        EventTextController.SetOverrideText("Press [K], or [LMB] to Fire");
        while (Input.GetAxis("Fire") == 0 && Input.GetAxis("Mouse") == 0) yield return null;
        EventTextController.CloseOverrideText();
        yield return new WaitForSeconds(3);

        EventTextController.SetOverrideText("Reload with [R]");
        while (Input.GetAxis("Reload") == 0) yield return null;
        EventTextController.CloseOverrideText();
        yield return new WaitForSeconds(3);

        EventTextController.SetOverrideText("Use your compass with [E]");
        while (Input.GetAxis("Compass") == 0) yield return null;
        EventTextController.CloseOverrideText();
        _seenControlsGuide = true;
        yield return new WaitForSeconds(10);
        EventTextController.SetOverrideText("When you are ready to leave, go to the edge of the region and press [T]");
        yield return new WaitForSeconds(2);
        EventTextController.CloseOverrideText();
    }

    public static bool Active()
    {
        return _tutorialActive;
    }
}