using Game.Combat.Generation;
using Game.Global;
using SamsHelper.Input;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.MenuSystem;
using TMPro;

public class CombatJournalController : Menu, IInputListener
{
    private static TextMeshProUGUI _title;
    private static TextMeshProUGUI _body;
    private static CloseButtonController _closeButton;
    private static bool _closing;

    public override void Awake()
    {
        base.Awake();
        _title = gameObject.FindChildWithName<TextMeshProUGUI>("Title");
        _body = gameObject.FindChildWithName<TextMeshProUGUI>("Text");
        _closeButton = gameObject.FindChildWithName<CloseButtonController>("Close Button");
    }

    public static void ShowJournal(JournalEntry journal)
    {
        _closing = false;
        MenuStateMachine.ShowMenu("Journal");
        _title.text = journal.Title;
        _body.text = journal.Contents;
    }

    public override void Enter()
    {
        InputHandler.SetCurrentListener(this);
        CombatManager.Pause();
    }

    private void Close()
    {
        if (_closing) return;
        _closing = true;
        _closeButton.Flash();
        CombatManager.Unpause();
        MenuStateMachine.ReturnToDefault();
    }

    public void OnInputDown(InputAxis axis, bool isHeld, float direction = 0)
    {
        if (axis != InputAxis.Cover || isHeld) return;
        Close();
    }

    public void OnInputUp(InputAxis axis)
    {
    }

    public void OnDoubleTap(InputAxis axis, float direction)
    {
    }
}