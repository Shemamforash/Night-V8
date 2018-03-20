using SamsHelper;
using TMPro;

namespace Game.Combat.CharacterUi
{
    public class PlayerUi : CharacterUi
    {
        public TextMeshProUGUI _playerName;

        public override void Awake()
        {
            base.Awake();
            _playerName = Helper.FindChildWithName<TextMeshProUGUI>(gameObject, "Name");
            HealthController.SetIsPlayerBar();
        }
    }
}