using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using EZCameraShake;
using Facilitating.Audio;
using Facilitating.Persistence;
using Game.Exploration.Environment;
using Game.Global;
using NUnit.Framework;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using SamsHelper.ReactiveUI.MenuSystem;
using TMPro;
using UnityEngine;
using Environment = Game.Exploration.Environment.Environment;
using Random = UnityEngine.Random;

public class StoryController : Menu
{
    private const float _timePerWord = 0.3f;
    private TextMeshProUGUI _storyText, _actTitle, _actSubtitle;
    private static bool _goToCredits;
    private static bool _paused;
    private bool _skipParagraph;
    private CanvasGroup _skipCanvas, _actCanvas;
    private CloseButtonController _closeButton;
    private AudioSource _audioSource;
    private bool _canSkip;
    private ParticleSystem _lightningSystem;
    private CameraShaker _shaker;
    private float _waitTime;
    private static List<JournalEntry> _journalEntries;
    private static readonly string[] _actNames = {"Act I", "Act II", "Act III", "Act IV", "Act V", "Epilogue"};
    private AudioSource _actAudio;
    public static bool StorySeen;
    private PostProcessInvertColour _invertColour;

    protected override void Awake()
    {
        base.Awake();
        _invertColour = Camera.main.GetComponent<PostProcessInvertColour>();
        if (EnvironmentManager.CurrentEnvironmentType() == EnvironmentType.End) _invertColour.Set(1);
        _actCanvas = GameObject.Find("Act").GetComponent<CanvasGroup>();
        _actAudio = _actCanvas.GetComponent<AudioSource>();
        _actTitle = _actCanvas.gameObject.FindChildWithName<TextMeshProUGUI>("Title");
        _actSubtitle = _actCanvas.gameObject.FindChildWithName<TextMeshProUGUI>("Subtitle");
        _storyText = GetComponent<TextMeshProUGUI>();
        _storyText.color = UiAppearanceController.InvisibleColour;
        _skipCanvas = GameObject.Find("Skip").GetComponent<CanvasGroup>();
        _closeButton = _skipCanvas.GetComponent<CloseButtonController>();
        _closeButton.SetCallback(Skip);
        _closeButton.SetOnClick(Skip);
        _closeButton.UseAcceptInput();
        _audioSource = Camera.main.GetComponent<AudioSource>();
        _audioSource.volume = 0f;
        _audioSource.DOFade(1f, 1f).SetUpdate(UpdateType.Normal, true);
        _skipCanvas.alpha = 0f;
        _paused = false;
        if (!_goToCredits) return;
        _waitTime = Random.Range(2f, 4f);
        _lightningSystem = GameObject.Find("Lightning System").GetComponent<ParticleSystem>();
        _shaker = GameObject.Find("Shaker").GetComponent<CameraShaker>();
    }

    public void Update()
    {
        if (!_goToCredits) return;
        _waitTime -= Time.deltaTime;
        if (_waitTime > 0f) return;
        float magnitude = Random.Range(4, 8);
        float roughness = Random.Range(10, 20);
        float inDuration = Random.Range(0.25f, 0.5f);
        float outDuration = Random.Range(0.25f, 0.5f);
        Sequence sequence = DOTween.Sequence();
        sequence.AppendCallback(() => _lightningSystem.Emit(1));
        sequence.AppendInterval(3f);
        sequence.AppendCallback(() =>
        {
            ThunderController.Instance().Thunder();
            _shaker.ShakeOnce(magnitude, roughness, inDuration, outDuration);
        });
        float totalDuration = inDuration + outDuration;
        _waitTime = Random.Range(totalDuration * 4f, totalDuration * 8f);
    }

    public override void Enter()
    {
        StartCoroutine(DisplayParagraph());
    }

    public static void Show()
    {
        StorySeen = false;
        _journalEntries = JournalEntry.GetStoryText();
        _goToCredits = EnvironmentManager.CurrentEnvironmentType() == EnvironmentType.End;
        SceneChanger.GoToStoryScene();
    }

    public static float GetTimeToRead(string paragraph)
    {
        int wordCount = paragraph.Split(' ').Length;
        float timeToRead = _timePerWord * wordCount;
        return timeToRead;
    }

    private IEnumerator DisplayParagraph()
    {
        yield return StartCoroutine(DisplayAct());
        Tweener skipTween = null;
        foreach (JournalEntry entry in _journalEntries)
        {
            //fade in
            _storyText.text = entry.Text + "\n\n    - <i>The Necromancer</i>";
            _storyText.color = UiAppearanceController.InvisibleColour;
            yield return _storyText.DOFade(1f, 1f).WaitForCompletion();

            skipTween?.Kill();
            skipTween = _skipCanvas.DOFade(0.5f, 1f);
            _closeButton.Enable();
            _canSkip = true;
            _skipCanvas.alpha = 1f;
            //read
            float timeToRead = GetTimeToRead(entry.Text);
            while (timeToRead > 0 && !_skipParagraph)
            {
                if (!_paused) timeToRead -= Time.deltaTime;
                yield return null;
            }

            _canSkip = false;
            _skipParagraph = false;
            _closeButton.Disable();
            skipTween?.Kill();
            skipTween = _skipCanvas.DOFade(0, 1f);

            //fade out
            _storyText.color = Color.white;
            yield return _storyText.DOFade(0f, 1f).WaitForCompletion();
        }

        _invertColour.FadeTo(0f, 0.5f);
        _audioSource.DOFade(0f, 0.5f).SetUpdate(UpdateType.Normal, true);
        End();
    }

    private IEnumerator DisplayAct()
    {
        _storyText.alpha = 0;
        EnvironmentType currentEnvironmentType = EnvironmentManager.CurrentEnvironmentType();
        _actTitle.text = _actNames[(int) currentEnvironmentType];
        _actSubtitle.text = Environment.EnvironmentTypeToName(currentEnvironmentType);
        _actAudio.Play();
        yield return _actCanvas.DOFade(1, 1f).WaitForCompletion();
        yield return new WaitForSeconds(2f);
        yield return _actCanvas.DOFade(0f, 1f).WaitForCompletion();
    }

    private void End()
    {
        StorySeen = true;
        SaveController.AutoSave();
        _closeButton.Disable();
        SceneChanger.FadeInAudio();
        if (_goToCredits) SceneChanger.GoToCreditsScene();
        else SceneChanger.GoToGameScene();
    }

    private void Skip()
    {
        if (_skipParagraph || !_canSkip) return;
        _skipParagraph = true;
        _skipCanvas.alpha = 0.8f;
    }

    public static void Pause()
    {
        _paused = true;
    }

    public static void Unpause()
    {
        _paused = false;
    }
}