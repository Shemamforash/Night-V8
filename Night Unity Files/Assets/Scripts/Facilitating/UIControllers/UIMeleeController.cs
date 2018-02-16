using Game.Combat;
using SamsHelper;
using SamsHelper.Input;
using TMPro;
using UnityEngine;

public class UIMeleeController : MonoBehaviour
{
    private const float TargetRadius = 50f;
    private const float OuterRadius = 300f;

    private const float OuterRingWidth = 0.01f, InnerRingWidth = 0.05f;
    private float _currentRingTime, _currentPressTime;
    
    private float _currentRadius;
    public InputAxis Axis;
    public bool NegativeDirection;
    private RingDrawer _outerRing;
    private GameObject _innerRing;
    private TextMeshProUGUI _keyText;
    private bool _running;

    public void Awake()
    {
        _outerRing = Helper.FindChildWithName<RingDrawer>(gameObject, "Outer Ring");
        _innerRing = Helper.FindChildWithName(gameObject, "Inner Ring");
        _keyText = Helper.FindChildWithName<TextMeshProUGUI>(gameObject, "Key");
    }

    public void Start()
    {
        _outerRing.SetLineWidth(OuterRingWidth);
        _outerRing.Hide();
        SetEnabled(false);
    }

    private void SetEnabled(bool enabled)
    {
        _outerRing.gameObject.SetActive(enabled);
        _innerRing.gameObject.SetActive(enabled);
        _keyText.gameObject.SetActive(enabled);
    }
    
    public void StartRunning()
    {
        SetEnabled(true);
        _running = true;
        _currentRingTime = MeleeController.InitialRingTime();
        _currentPressTime = MeleeController.InitialPressTime();
        _currentRadius = OuterRadius;
//        _innerRing.DrawCircle(TargetRadius);
//        _innerRing.SetFaded();
    }
    
    public void Update()
    {
        if (!_running) return;
        if (_currentRingTime > 0)
        {
            MoveOuterRing();
        }
        else if (_currentPressTime > 0)
        {
            CheckForInput();  
        }
        else
        {
            EndMelee(false);
        }
    }

    private void MoveOuterRing()
    {
        float outerRingAlpha = _currentRingTime / MeleeController.InitialRingTime();
        _currentRadius = (OuterRadius - TargetRadius) * outerRingAlpha + TargetRadius;
        outerRingAlpha = 1 - outerRingAlpha;
        Color c = new Color(1, 1, 1, outerRingAlpha);
        _outerRing.SetColor(c);
        _outerRing.DrawCircle(_currentRadius);
        _currentRingTime -= Time.deltaTime;
    }

    private void CheckForInput()
    {
//        _innerRing.SetHighlighted();
        _outerRing.Hide();
        float axis = Input.GetAxis(Axis.ToString());
        if (NegativeDirection && axis < 0 ||
            !NegativeDirection && axis > 0)
        {
            EndMelee(true);
        }
        _currentPressTime -= Time.deltaTime;
    }

    private void EndMelee(bool success)
    {
        _running = false;
        SetEnabled(false);
        if (success)
        {
            transform.Find("Particle Spray").GetComponent<ParticleSystem>().Emit(100);
            MeleeController.SucceedRound();
        }
        else
        {
            MeleeController.FailRound();
        }
    }
}