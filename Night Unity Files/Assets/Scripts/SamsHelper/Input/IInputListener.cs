namespace SamsHelper.Input
{
    public interface IInputListener
    {
        void OnInputDown(InputAxis axis, bool isHeld, float direction = 0);
        void OnInputUp(InputAxis axis);
        void OnDoubleTap(InputAxis axis, float direction);
    }
}