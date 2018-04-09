namespace Facilitating.UI
{
    public class Toggler : Highlight
    {
        public override void Awake()
        {
            base.Awake();
            On();
        }

        public void Toggle()
        {
            if (childTexts[0].text.ToLower() == "on")
                Off();
            else
                On();
        }

        protected virtual void On()
        {
            childTexts[0].text = "ON";
        }

        protected virtual void Off()
        {
            childTexts[0].text = "OFF";
        }
    }
}