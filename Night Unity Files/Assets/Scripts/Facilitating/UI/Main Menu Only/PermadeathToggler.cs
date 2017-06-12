namespace UI.Highlight
{
    using Persistence;

    public class PermadeathToggler : Toggler
    {
        protected override void On()
        {
            base.On();
            Settings.permadeathOn = true;
        }

        protected override void Off()
        {
            base.Off();
            Settings.permadeathOn = false;
        }
    }
}
