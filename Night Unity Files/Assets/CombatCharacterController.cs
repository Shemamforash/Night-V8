using Game.Combat;
using SamsHelper;
using SamsHelper.Input;
using UnityEngine;

public class CombatCharacterController : MonoBehaviour, IInputListener
{
    private CharacterCombat _owner;
    private Rigidbody2D _rigidbody;
    private Vector2 _velocity = new Vector2(0, 0);
    private const float Acceleration = 10f;
    private const float Deceleration = 0.9f;
    private Transform _pivot;
    private bool _movingHorizontally, _movingVertically;

    public void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
    }

    public void SetOwner(CharacterCombat owner)
    {
        _owner = owner;
        if (!(_owner is PlayerCombat)) return;
        _pivot = Helper.FindChildWithName<Transform>(gameObject, "Pivot");
        InputHandler.RegisterInputListener(this);
    }

    public CharacterCombat Owner()
    {
        return _owner;
    }

    private void UpdateHorizontalVelocity()
    {
        _velocity.x *= Deceleration;
        if (Mathf.Abs(_velocity.x) < 0.01f)
        {
            _velocity.x = 0;
        }

        _rigidbody.velocity = _velocity;
    }

    private void UpdateVerticalVelocity()
    {
        _velocity.y *= Deceleration;
        if (Mathf.Abs(_velocity.y) < 0.01f)
        {
            _velocity.y = 0;
        }

        _rigidbody.velocity = _velocity;
    }

    private void FixedUpdate()
    {
        if (!_movingHorizontally || _dashing) UpdateHorizontalVelocity();
        if (!_movingVertically || _dashing) UpdateVerticalVelocity();
        if (_velocity.magnitude < _owner.Speed) _dashing = false;
    }

    public void Update()
    {
        GetComponent<FootstepMaker>().UpdateRotation(AdvancedMaths.AngleFromUp(Vector3.zero, _rigidbody.velocity.normalized));
        if (_owner.GetTarget() == null) return;
        if (!(_owner is PlayerCombat)) return;
        _pivot.rotation = AdvancedMaths.RotationToTarget(transform.position, _owner.GetTarget().CharacterController.transform.position);
    }

    public Vector3 Position()
    {
        return transform.position;
    }

    private bool _dashing;

    private void DashHorizontal(float direction)
    {
        _dashing = true;
        _velocity.x += direction * 5;
        _rigidbody.velocity = _velocity;
    }

    private void DashVertical(float direction)
    {
        _dashing = true;
        _velocity.y += direction * 5;
        _rigidbody.velocity = _velocity;
    }

    private void MoveHorizontal(float speed, float direction)
    {
        _velocity.x += Time.deltaTime * Acceleration * direction;
        if (Mathf.Abs(_velocity.x) > speed) _velocity.x = speed * direction;
        _rigidbody.velocity = _velocity;
    }

    private void MoveVertical(float speed, float direction)
    {
        _velocity.y += Time.deltaTime * Acceleration * direction;
        if (Mathf.Abs(_velocity.y) > speed) _velocity.y = speed * direction;
        _rigidbody.velocity = _velocity;
    }

    public void SetDistance(int rangeMin, int rangeMax)
    {
        Vector3 position = new Vector3();
        position.x = Random.Range(rangeMin, rangeMax);
        if (Random.Range(0, 2) == 1) position.x = -position.x;
        position.y = Random.Range(rangeMin, rangeMax);
        if (Random.Range(0, 2) == 1) position.y = -position.y;
        transform.position = position;
    }

    private void Move(InputAxis axis, float direction)
    {
        if (_dashing) return;
        float speed = _owner.Speed;
        if (_owner.Sprinting) speed *= CharacterCombat.SprintModifier;

        if (axis == InputAxis.Horizontal)
        {
            MoveHorizontal(speed, direction);
        }
        else
        {
            MoveVertical(speed, direction);
        }
    }

    public void OnInputDown(InputAxis axis, bool isHeld, float direction = 0)
    {
        if (_owner.Immobilised()) return;
        if (!isHeld) return;
        switch (axis)
        {
            case InputAxis.Horizontal:
                _movingHorizontally = true;
                Move(axis, direction);
                break;
            case InputAxis.Vertical:
                _movingVertically = true;
                Move(axis, direction);
                break;
        }
    }

    public void OnInputUp(InputAxis axis)
    {
        switch (axis)
        {
            case InputAxis.Horizontal:
                _movingHorizontally = false;
                break;
            case InputAxis.Vertical:
                _movingVertically = false;
                break;
        }
    }

    public void OnDoubleTap(InputAxis axis, float direction)
    {
        switch (axis)
        {
            case InputAxis.Horizontal:
                DashHorizontal(direction);
                break;
            case InputAxis.Vertical:
                DashVertical(direction);
                break;
        }
    }
}