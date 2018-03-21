using Game.Combat;
using SamsHelper;
using SamsHelper.Input;
using UnityEngine;

public class CombatCharacterController : MonoBehaviour, IInputListener
{
    private CharacterCombat _owner;
    private Rigidbody2D _rigidbody;
    private Transform _pivot;

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

    private void DashHorizontal(float direction)
    {
        _rigidbody.AddForce(new Vector2(direction * 300f, 0f));
    }

    private void DashVertical(float direction)
    {
        _rigidbody.AddForce(new Vector2(0f, direction * 300f));
    }

    private void MoveHorizontal(float speed, float direction)
    {
        _rigidbody.AddForce(new Vector2(direction * speed * 2, 0f));
    }

    private void MoveVertical(float speed, float direction)
    {
        _rigidbody.AddForce(new Vector2(0f, direction * speed * 2));
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
                Move(axis, direction);
                break;
            case InputAxis.Vertical:
                Move(axis, direction);
                break;
        }
    }

    public void OnInputUp(InputAxis axis)
    {
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