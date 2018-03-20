using Facilitating.UIControllers;
using SamsHelper;
using UnityEngine;

namespace Game.Combat.CharacterUi
{
    public class CharacterUi : MonoBehaviour
    {
        public UIHealthBarController HealthController;
        public UIArmourController ArmourController;
        public CanvasGroup CanvasGroup;

        public virtual void Awake()
        {
            CanvasGroup = gameObject.GetComponent<CanvasGroup>();
            ArmourController = Helper.FindChildWithName<UIArmourController>(gameObject, "Armour");
            HealthController = Helper.FindChildWithName<UIHealthBarController>(gameObject, "Health");
        }
    }
}