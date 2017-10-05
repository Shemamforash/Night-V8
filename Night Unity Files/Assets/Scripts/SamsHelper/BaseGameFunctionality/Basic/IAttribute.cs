namespace SamsHelper.BaseGameFunctionality.Basic
{
    public interface IAttribute
    {
        float CalculatedValue();
        void AddModifier(float modifier, bool summative = false);
        void RemoveModifier(float modifier, bool summative = false);
        void Recalculate();
    }
}