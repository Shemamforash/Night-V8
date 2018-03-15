using Game.Combat;
using SamsHelper;
using UnityEngine;

public class CombatCharacterController : MonoBehaviour
{
    private CharacterCombat _owner;
    private bool _followOwner;
    private Rigidbody2D _rigidbody;

    public void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
    }
    
    public void SetOwner(CharacterCombat owner)
    {
        _owner = owner;
        if (_owner is PlayerCombat) _followOwner = true;
    }

    public CharacterCombat Owner()
    {
        return _owner;
    }
    
    public void FixedUpdate()
    {
        if (_followOwner)
        {
            Vector3 cameraPos = transform.position;
            cameraPos.z = -10;
//            Camera.main.transform.position = cameraPos;
        }
        if (_owner.GetTarget() == null) return;
        float angle = -AdvancedMaths.AngleFromUp(transform, _owner.GetTarget().CharacterController.transform);
        Vector3 currentRotation = transform.rotation.eulerAngles;
        currentRotation.z = angle;
//        transform.rotation = Quaternion.Euler(currentRotation);
    }

    public Vector3 Position()
    {
        return transform.position;
    }

    public void MoveLeft(float speed)
    {
        Vector2 currentVelocity = _rigidbody.velocity;
        currentVelocity.x = -1 * speed;
        _rigidbody.velocity = currentVelocity;

//        Vector3 newPosition = transform.position;
//        newPosition.x -= speed * Time.deltaTime;
//        _rigidbody.MovePosition(newPosition);
    }

    public void MoveRight(float speed)
    {
        Vector2 currentVelocity = _rigidbody.velocity;
        currentVelocity.x = 1 * speed;
        _rigidbody.velocity = currentVelocity;

//        Vector3 newPosition = transform.position;
//        newPosition.x += speed * Time.deltaTime;
//        _rigidbody.MovePosition(newPosition);
    }

    public void MoveUp(float speed)
    {
        Vector2 currentVelocity = _rigidbody.velocity;
        currentVelocity.y = 1 * speed;
        _rigidbody.velocity = currentVelocity;
//        Vector3 newPosition = transform.position;
//        newPosition.y += speed * Time.deltaTime;
//        _rigidbody.MovePosition(newPosition);
    }

    public void MoveDown(float speed)
    {
        Vector2 currentVelocity = _rigidbody.velocity;
        currentVelocity.y = -1 * speed;
        _rigidbody.velocity = currentVelocity;
//        Vector3 newPosition = transform.position;
//        newPosition.y -= speed * Time.deltaTime;
//        _rigidbody.MovePosition(newPosition);
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
}