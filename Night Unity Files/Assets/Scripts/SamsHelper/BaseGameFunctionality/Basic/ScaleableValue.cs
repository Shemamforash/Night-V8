using UnityEngine;

namespace SamsHelper.BaseGameFunctionality.Basic
{
    public class ScaleableValue
    {
        private readonly float _xSquaredCoefficient, _xCoefficient, _intercept;
            
        public ScaleableValue(float xCoefficient, float intercept)
        {
            _xCoefficient = xCoefficient;
            _intercept = intercept;
        }

        public ScaleableValue(float xSquaredCoefficient, float xCoefficient, float intercept) : this(xCoefficient, intercept)
        {
            _xSquaredCoefficient = xSquaredCoefficient;
        }

        public float GetScaledValue(float value)
        {
            Debug.Log(value + " " + _xCoefficient +" " +_intercept);
            return _xSquaredCoefficient * value * value +_xCoefficient * value + _intercept;
        }
    }
}