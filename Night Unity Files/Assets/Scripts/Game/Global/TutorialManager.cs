using System.Collections.Generic;
using System.Linq;
using System.Xml;
using DG.Tweening;
using Facilitating.Persistence;
using Game.Combat.Generation;
using Game.Global;
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

    public static void TryOpenTutorial(int tutorialPart)
    {
        if (!_tutorialActive) return;
        if (_showingTutorial) return;
        ReadTutorialParts();
        _currentTutorialPart = _tutorialParts[tutorialPart].FirstOrDefault(p => !p.IsComplete());
        if (_currentTutorialPart == null) return;
        EnableButton();
        ShowTutorialPart();
        Enter();
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
        _overlayController.SetTutorialArea(_currentTutorialPart.MinOffset, _currentTutorialPart.MaxOffset);
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
        _alreadyHidden = ResourcesUiController.Hidden();
        if (!_alreadyHidden) ResourcesUiController.Hide();
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

    private class TutorialPart
    {
        public readonly Vector2 MinOffset, MaxOffset;
        public readonly string Title, Content;
        public readonly int SectionNumber, PartNumber;
        private bool _completed;
        private TutorialPart _nextPart;

        public TutorialPart(XmlNode node)
        {
            MinOffset = node.StringFromNode("MinOffset").ToVector2();
            MaxOffset = node.StringFromNode("MaxOffset").ToVector2();
            Title = node.StringFromNode("Title");
            Content = node.StringFromNode("Text");
            Content = Content.Replace("\n", "");
            Content = Content.Replace(". ", ".\n");
            PartNumber = node.IntFromNode("PartNumber");
            SectionNumber = node.IntFromNode("SectionNumber");
        }

        public void SaveTutorialPart(XmlNode root)
        {
            root = root.CreateChild("Part");
            root.CreateChild("PartNumber", PartNumber);
            root.CreateChild("Completed", _completed);
        }

        public void SetNextPart(TutorialPart nextPart)
        {
            _nextPart = nextPart;
        }

        public TutorialPart NextPart() => _nextPart;

        public void MarkComplete() => _completed = true;

        public bool IsComplete() => _completed;
    }

    public static void SetTutorialActive(bool tutorialActive)
    {
        _tutorialActive = tutorialActive;
    }
}