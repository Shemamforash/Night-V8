using System;
using System.Collections.Generic;
using DefaultNamespace;
using Game.Global;
using Game.Global.Tutorial;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using SamsHelper.ReactiveUI.MenuSystem;
using UnityEngine;

public class HelpMenuController : Menu
{
    private ListController _tutorialList;
    private static EnhancedText _tutorialText;

    protected override void Awake()
    {
        base.Awake();
        _tutorialList = gameObject.FindChildWithName<ListController>("List");
        _tutorialText = gameObject.FindChildWithName<EnhancedText>("Help Text");
    }

    public override void Enter()
    {
        base.Enter();
        _tutorialList.Initialise(typeof(TutorialElement), o => { }, () => { }, GetAvailableTutorials);
        _tutorialList.SetOnItemHover(UpdateTutorialShown);
        _tutorialList.Show();
    }

    private List<object> GetAvailableTutorials()
    {
        Dictionary<int, List<TutorialPart>> tutorialParts = TutorialManager.GetTutorialParts();
        List<Tuple<string, string>> visibleTutorialParts = new List<Tuple<string, string>>();
        foreach (int section in tutorialParts.Keys)
        {
            string fullTutorial = "";
            List<TutorialPart> parts = tutorialParts[section];
            for (int i = 0; i < parts.Count; ++i)
            {
                if (!parts[i].IsViewable()) continue;
                if (i > 0) fullTutorial += "\n\n";
                fullTutorial += parts[i].Content;
            }

            visibleTutorialParts.Add(Tuple.Create(parts[0].SectionName, fullTutorial));
        }

        return new List<object>(visibleTutorialParts);
    }

    private void UpdateTutorialShown(object o)
    {
        Tuple<string, string> tutorial = (Tuple<string, string>) o;
        _tutorialText.SetText(tutorial.Item2);
    }

    private class TutorialElement : ListElement
    {
        private EnhancedText _nameText;

        protected override void SetVisible(bool visible)
        {
            _nameText.SetColor(visible ? Color.white : UiAppearanceController.InvisibleColour);
        }

        protected override void CacheUiElements(Transform transform)
        {
            _nameText = transform.GetComponent<EnhancedText>();
        }

        protected override void Update(object o, bool isCentreItem)
        {
            Tuple<string, string> tutorial = (Tuple<string, string>) o;
            _nameText.SetText(tutorial.Item1);
        }

        protected override void UpdateCentreItemEmpty()
        {
            _nameText.SetText("");
            _tutorialText.SetText("");
        }

        public override void SetColour(Color c)
        {
            _nameText.SetColor(c);
        }
    }
}