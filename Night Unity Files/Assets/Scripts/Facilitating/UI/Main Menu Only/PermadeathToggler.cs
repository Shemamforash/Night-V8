namespace UI.Highlight
{
    using Persistence;

    public class PermadeathToggler : Toggler
    {
        private bool permadeathOn = false;
        private PersistenceListener persistenceListener;

        public override void Awake()
        {
            base.Awake();
            persistenceListener = new PersistenceListener(() => permadeathOn = GameData.permadeathOn, () => GameData.permadeathOn = permadeathOn, "Permadeath Toggler");
        }

        protected override void On()
        {
            base.On();
            permadeathOn = true;
        }

        protected override void Off()
        {
            base.Off();
            permadeathOn = false;
        }
    }
}
