namespace Game.Combat.Ui
{
    public class PlayerUi : CharacterUi
    {
        private static PlayerUi _instance;

        public override void Awake()
        {
            base.Awake();
            _instance = this;
            _healthBarController.SetIsPlayerBar();
        }

        public static PlayerUi Instance()
        {
            if (_instance != null) return _instance;
            _instance = FindObjectOfType<PlayerUi>();
            return _instance;
        }
    }
}