using SamsHelper;
using SamsHelper.Input;
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
    private RingDrawer _outerRing, _innerRing;
    private bool _running;

    public void Awake()
    {
        _outerRing = Helper.FindChildWithName<RingDrawer>(gameObject, "Outer Ring");
        _innerRing = Helper.FindChildWithName<RingDrawer>(gameObject, "Inner Ring");
    }

    public void Start()
    {
        _outerRing.SetLineWidth(OuterRingWidth);
        _innerRing.SetLineWidth(InnerRingWidth);
        _outerRing.Hide();
        _innerRing.Hide();
    }

    public void StartRunning()
    {
        _running = true;
        _currentRingTime = MeleeController.InitialRingTime();
        _currentPressTime = MeleeController.InitialPressTime();
        _currentRadius = OuterRadius;
        _innerRing.DrawCircle(TargetRadius);
        _innerRing.SetFaded();
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
        _innerRing.SetHighlighted();
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
        if (success)
        {
            MeleeController.SucceedRound();
        }
        else
        {
            MeleeController.ExitMelee();
        }
    }
}