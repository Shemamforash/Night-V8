using Game.Global;
using SamsHelper.Input;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.MenuSystem;
using TMPro;

public class CombatJournalController : Menu, IInputListener {
    private static TextMeshProUGUI _title;
    private static TextMeshProUGUI _body;

    public override void Awake()
    {
        base.Awake();
        _title = gameObject.FindChildWithName<TextMeshProUGUI>("Title");
        _body = gameObject.FindChildWithName<TextMeshProUGUI>("Text");
    }

    public static void ShowJournal(JournalEntry journal)
    {
        MenuStateMachine.ShowMenu("Journal");
        _title.text = journal.Title;
        _body.text = journal.Contents;
    }

    public override void Enter()
    {
        InputHandler.SetCurrentListener(this);
    }    
    
    private void Close()
    {
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
