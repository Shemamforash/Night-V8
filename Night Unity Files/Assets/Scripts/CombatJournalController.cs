using Game.Combat.Generation;
using Game.Global;
using SamsHelper.Input;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.MenuSystem;
using TMPro;
using UnityEngine;

public class CombatJournalController : Menu, IInputListener
{
    private static TextMeshProUGUI _title;
    private static TextMeshProUGUI _body;
    private static CloseButtonController _closeButton;
    private static bool _closing;
    private AudioSource _audioSource;

    public override void Awake()
    {
        base.Awake();
        _title = gameObject.FindChildWithName<TextMeshProUGUI>("Title");
        _body = gameObject.FindChildWithName<TextMeshProUGUI>("Text");
        _closeButton = gameObject.FindChildWithName<CloseButtonController>("Close Button");
        _closeButton.SetOnClick(Close);
        _audioSource = GetComponent<AudioSource>();
        _audioSource.clip = AudioClips.OpenJournal;
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
        _audioSource.Play();
    }

    private void Close()
    {
        if (_closing) return;
        _closing = true;
        _closeButton.Flash();
        CombatManager.Resume();
        MenuStateMachine.ReturnToDefault();
    }

    public void OnInputDown(InputAxis axis, bool isHeld, float direction = 0)
    {
        if (axis != InputAxis.Fire || isHeld) return;
        Close();
    }

    public void OnInputUp(InputAxis axis)
    {
    }

    public void OnDoubleTap(InputAxis axis, float direction)
    {
    }
}