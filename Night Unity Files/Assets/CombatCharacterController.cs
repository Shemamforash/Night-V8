using Game.Combat;
using SamsHelper;
using UnityEngine;

public class CombatCharacterController : MonoBehaviour
{
    private CharacterCombat _owner;
    private bool _followOwner;

    public void SetOwner(CharacterCombat owner)
    {
        _owner = owner;
        if (_owner is PlayerCombat) _followOwner = true;
    }

    public CharacterCombat Owner()
    {
        return _owner;
    }
    
    public void Update()
    {
        if (_followOwner)
        {
            Vector3 cameraPos = transform.position;
            cameraPos.z = -10;
            Camera.main.transform.position = cameraPos;
        }
        if (_owner.GetTarget() == null) return;
        float angle = -AdvancedMaths.AngleFromUp(transform, _owner.GetTarget().CharacterController.transform);
        Vector3 currentRotation = transform.rotation.eulerAngles;
        currentRotation.z = angle;
        transform.rotation = Quaternion.Euler(currentRotation);
    }

    public Vector3 Position()
    {
        return transform.position;
    }

    public void MoveLeft(float speed)
    {
        Vector3 newPosition = transform.position;
        newPosition.x -= speed * Time.deltaTime;
        transform.position = newPosition;
    }

    public void MoveRight(float speed)
    {
        Vector3 newPosition = transform.position;
        newPosition.x += speed * Time.deltaTime;
        transform.position = newPosition;
    }

    public void MoveUp(float speed)
    {
        Vector3 newPosition = transform.position;
        newPosition.y += speed * Time.deltaTime;
        transform.position = newPosition;
    }

    public void MoveDown(float speed)
    {
        Vector3 newPosition = transform.position;
        newPosition.y -= speed * Time.deltaTime;
        transform.position = newPosition;
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