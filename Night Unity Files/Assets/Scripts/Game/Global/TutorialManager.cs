using System.Collections.Generic;
using System.Linq;
using System.Xml;
using DG.Tweening;
using Facilitating.Persistence;
using Game.Combat.Generation;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using SamsHelper.ReactiveUI.MenuSystem;
using UnityEngine;

public class TutorialManager : Menu
{
    private static readonly Dictionary<int, List<TutorialPart>> _tutorialParts = new Dictionary<int, List<TutorialPart>>();
    private static TutorialPart _currentTutorialPart;
    private static bool _loaded;
    private static EnhancedText _titleText, _contentText;
    private static TutorialOverlayController _overlayController;
    private static CloseButtonController _closeButton;

    public override void Awake()
    {
        base.Awake();
        GameObject top = gameObject.FindChildWithName("Instructions Top");
        _titleText = top.FindChildWithName<EnhancedText>("Title");
        _contentText = top.FindChildWithName<EnhancedText>("Text");
        _overlayController = gameObject.GetComponent<TutorialOverlayController>();
        _closeButton = top.FindChildWithName<CloseButtonController>("Close Button");
    }

    public void Start()
    {
        MenuStateMachine.RegisterMenu(this);
    }

    public static void TryOpenTutorial(int tutorialPart)
    {
        ReadTutorialParts();
        MenuStateMachine.ShowMenu("Tutorial Overlay");
        _closeButton.SetCallback(ShowTutorialPart);
        _currentTutorialPart = _tutorialParts[tutorialPart].First(p => !p.IsComplete());
        _closeButton.SetText("K");
        ShowTutorialPart();
    }

    private static void ShowTutorialPart()
    {
        if (_currentTutorialPart.NextPart() == null)
        {
            _closeButton.SetCallback(Close);
            _closeButton.SetText("C");
        }

        string title = _currentTutorialPart.Title;
        string content = _currentTutorialPart.Content;
        _titleText.SetText(title);
        _contentText.SetText(content);
        _overlayController.SetTutorialArea(_currentTutorialPart.MinOffset, _currentTutorialPart.MaxOffset);
        _currentTutorialPart.MarkComplete();
    }

    public override void Enter()
    {
        base.Enter();
        CombatManager.Pause();
        DOTween.defaultTimeScaleIndependent = true;
    }

    private static void Close()
    {
        CombatManager.Unpause();
        MenuStateMachine.ReturnToDefault();
    }

    public static void Load(XmlNode root)
    {
        ReadTutorialParts();
        root = root.SelectSingleNode("TutorialParts");
        foreach (int section in _tutorialParts.Keys)
            _tutorialParts[section].ForEach(p => p.LoadTutorialPart(root));
    }

    public static void Save(XmlNode root)
    {
        root = root.CreateChild("TutorialParts");
        foreach (int section in _tutorialParts.Keys)
            _tutorialParts[section].ForEach(p => p.SaveTutorialPart(root));
    }

    private static void ReadTutorialParts()
    {
        if (_loaded) return;
        XmlNode root = Helper.OpenRootNode("Tutorials", "Tutorials");
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
            PartNumber = node.IntFromNode("PartNumber");
            SectionNumber = node.IntFromNode("SectionNumber");
        }

        public void LoadTutorialPart(XmlNode root)
        {
            root = root.SelectSingleNode(PartNumber.ToString());
            _completed = root.BoolFromNode("Completed");
        }

        public void SaveTutorialPart(XmlNode root)
        {
            root = root.CreateChild(PartNumber.ToString());
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
}