namespace SamsHelper.BaseGameFunctionality.Basic
{
    public interface IAttribute
    {
        float GetCalculatedValue();
        void AddModifier(float modifier, bool summative = false);
        void RemoveModifier(float modifier, bool summative = false);
    }
}