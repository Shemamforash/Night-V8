using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using EZCameraShake;
using Facilitating.Audio;
using Facilitating.Persistence;
using Game.Exploration.Environment;
using Game.Exploration.Ui;
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
    private TextMeshProUGUI _storyText, _actTitle, _actSubtitle, _pageCountText;
    private static bool _goToCredits;
    private static bool _paused;
    private bool _skipParagraph;
    private CanvasGroup _skipCanvas, _actCanvas, _storyCanvas;
    private CloseButtonController _closeButton;
    private AudioSource _droneAudioSource;
    private bool _canSkip;
    private ParticleSystem _lightningSystem;
    private CameraShaker _shaker;
    private float _waitTime;
    private static List<JournalEntry> _journalEntries;
    private static readonly string[] _actNames = {"Act I", "Act II", "Act III", "Act IV", "Act V", "The End"};
    private AudioSource _actAudio;
    public static bool StorySeen;
    private PostProcessInvertColour _invertColour;
    private float _thunderVolumeOffset = 0f;
    private bool _showLightning = false;

    protected override void Awake()
    {
        base.Awake();
        CacheComponents();
        InitialiseComponents();
        SetEndGameValues();
    }

    private void Start()
    {
        if (EnvironmentManager.CurrentEnvironmentType == EnvironmentType.End)
        {
            _closeButton.gameObject.SetActive(false);
            return;
        }

        _closeButton.UseSpaceInput();
    }

    public void Update()
    {
        if (!_goToCredits) return;
        if (!_showLightning) return;
        _waitTime -= Time.deltaTime;
        if (_waitTime > 0f) return;
        float magnitude = Random.Range(4, 8);
        float roughness = Random.Range(10, 20);
        float inDuration = Random.Range(0.25f, 0.5f);
        float outDuration = Random.Range(0.25f, 0.5f);
        Sequence sequence = DOTween.Sequence();
        sequence.AppendCallback(() => _lightningSystem.Emit(1));
        sequence.AppendInterval(2f);
        sequence.AppendCallback(() =>
        {
            ThunderController.Instance().Thunder(_thunderVolumeOffset);
            _shaker.ShakeOnce(magnitude, roughness, inDuration, outDuration);
        });
        float totalDuration = inDuration + outDuration;
        _waitTime = Random.Range(totalDuration * 4f, totalDuration * 8f);
    }

    private void CacheComponents()
    {
        _invertColour = Camera.main.GetComponent<PostProcessInvertColour>();
        _actCanvas = GameObject.Find("Act").GetComponent<CanvasGroup>();
        _actAudio = _actCanvas.GetComponent<AudioSource>();
        _actTitle = _actCanvas.gameObject.FindChildWithName<TextMeshProUGUI>("Title");
        _actSubtitle = _actCanvas.gameObject.FindChildWithName<TextMeshProUGUI>("Subtitle");
        _storyCanvas = GetComponent<CanvasGroup>();
        _storyText = GetComponent<TextMeshProUGUI>();
        _pageCountText = _storyText.FindChildWithName<TextMeshProUGUI>("Page Count");
        _pageCountText.text = "";
        _skipCanvas = GameObject.Find("Skip").AddComponent<CanvasGroup>();
        _skipCanvas.alpha = 0f;
        _closeButton = _skipCanvas.GetComponent<CloseButtonController>();
        _droneAudioSource = Camera.main.GetComponent<AudioSource>();
    }

    private void InitialiseComponents()
    {
        _closeButton.SetOnClick(Skip);
        _droneAudioSource.volume = 0f;
        _skipCanvas.alpha = 0f;
        _paused = false;
        if (EnvironmentManager.CurrentEnvironmentType == EnvironmentType.End)
        {
            _invertColour.Set(1);
            return;
        }

        _droneAudioSource.DOFade(1f, 1f).SetUpdate(UpdateType.Normal, true);
    }

    private void SetEndGameValues()
    {
        if (!_goToCredits) return;
        _waitTime = Random.Range(2f, 4f);
        _lightningSystem = GameObject.Find("Lightning System").GetComponent<ParticleSystem>();
        _shaker = GameObject.Find("Shaker").GetComponent<CameraShaker>();
        transform.parent.FindChildWithName<AudioSource>("Act").volume = 0;
    }

    public override void Enter()
    {
        if (EnvironmentManager.CurrentEnvironmentType == EnvironmentType.End) StartCoroutine(DisplayEpilogue());
        else StartCoroutine(DisplayParagraph());
    }

    public static void Show()
    {
        StorySeen = false;
        _journalEntries = JournalEntry.GetStoryText();
        _goToCredits = EnvironmentManager.CurrentEnvironmentType == EnvironmentType.End;
        SceneChanger.GoToStoryScene();
    }

    public static float GetTimeToRead(string paragraph)
    {
        int wordCount = paragraph.Split(' ').Length;
        float timeToRead = _timePerWord * wordCount;
        return timeToRead;
    }

    private IEnumerator DisplayEpilogue()
    {
        DOTween.To(() => _thunderVolumeOffset, f => _thunderVolumeOffset = f, 0f, _journalEntries.Count * 20f);
        for (int i = 0; i < _journalEntries.Count; i++)
        {
            if (i == _journalEntries.Count - 2) _showLightning = false;
            JournalEntry entry = _journalEntries[i];
            //fade in
            _storyText.text = entry.Text + "\n\n    - <i>The Wanderer</i>";
            int pageNo = i + 1;
            _pageCountText.text = pageNo + "/" + _journalEntries.Count;

            yield return _storyCanvas.DOFade(1f, 1f).WaitForCompletion();
            yield return new WaitForSeconds(18f);
            yield return _storyCanvas.DOFade(0f, 1f).WaitForCompletion();
        }

        yield return StartCoroutine(DisplayAct());

        _invertColour.FadeTo(0f, 0.5f);
        _droneAudioSource.DOFade(0f, 0.5f).SetUpdate(UpdateType.Normal, true);
        End();
    }

    private IEnumerator DisplayParagraph()
    {
        yield return StartCoroutine(DisplayAct());
        Tweener skipTween = null;
        for (int i = 0; i < _journalEntries.Count; i++)
        {
            JournalEntry entry = _journalEntries[i];
//fade in
            _storyText.text = entry.Text + "\n\n    - <i>The Necromancer</i>";
            int pageNo = i + 1;
            _pageCountText.text = pageNo + "/" + _journalEntries.Count;

            yield return _storyCanvas.DOFade(1f, 1f).WaitForCompletion();

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

            yield return _storyCanvas.DOFade(0f, 1f).WaitForCompletion();
        }

        _invertColour.FadeTo(0f, 0.5f);
        _droneAudioSource.DOFade(0f, 0.5f).SetUpdate(UpdateType.Normal, true);
        End();
    }

    private IEnumerator DisplayAct()
    {
        _storyCanvas.alpha = 0;
        EnvironmentType currentEnvironmentType = EnvironmentManager.CurrentEnvironmentType;
        _actTitle.text = _actNames[(int) currentEnvironmentType];
        _actSubtitle.text = Environment.EnvironmentTypeToName(currentEnvironmentType);
        if (currentEnvironmentType != EnvironmentType.End) _actAudio.Play();
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