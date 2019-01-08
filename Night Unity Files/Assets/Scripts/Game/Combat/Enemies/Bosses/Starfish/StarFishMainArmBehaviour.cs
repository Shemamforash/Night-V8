using SamsHelper.Libraries;
using UnityEngine;

public class StarFishMainArmBehaviour : MonoBehaviour
{
    private float _currentAngle;
    private StarFishArmBehaviour _firstArmSegment;
    private bool _armActive = true;

    private void Awake()
    {
        float angleOffset = transform.rotation.eulerAngles.z + 90f;
        _firstArmSegment = gameObject.FindChildWithName<StarFishArmBehaviour>("Arm 1");
        _firstArmSegment.SetOffset(angleOffset, transform, transform, 2, 1);
    }

    public void UpdateAngle(float zAngle)
    {
        if (!_armActive) return;
        if (AllArmsDead(_firstArmSegment))
        {
            _armActive = false;
            ExplodeArms(_firstArmSegment);
        }

        if (_firstArmSegment == null) return;
        _firstArmSegment.SetPosition(zAngle);
    }

    private bool AllArmsDead(StarFishArmBehaviour arm)
    {
        if (arm == null) return true;
        return arm.Dead() && AllArmsDead(arm.NextArm());
    }

    private void ExplodeArms(StarFishArmBehaviour arm)
    {
        while (true)
        {
            if (arm == null) return;
            LeafBehaviour.CreateLeaves(arm.transform.position);
            StarFishArmBehaviour lastArm = arm;
            arm = arm.NextArm();
            Destroy(lastArm.gameObject);
        }
    }
}