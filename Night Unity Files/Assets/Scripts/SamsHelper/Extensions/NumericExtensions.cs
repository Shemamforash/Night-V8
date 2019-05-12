using System;
using System.Globalization;
using Random = UnityEngine.Random;

namespace Extensions
{
	public static class NumericExtensions
	{
		public static bool RollDie(float target, float max) => Random.Range(0, max) == target;

		public static bool RollDie(int target, int max) => Random.Range(0, max) == target;

		public static float Round(this float val, int precision = 0)
		{
			float precisionDivider = (float) Math.Pow(10f, precision);
			return (float) (Math.Round(val * precisionDivider) / precisionDivider);
		}

		public static bool IsPolarityEqual(this float a, float b)
		{
			if (a > 0 && b <= 0 || a < 0 && b >= 0) return false;
			return true;
		}

		public static float Normalise(this float value, float maxValue) => value / maxValue;

		public static string AddSign(this float value)
		{
			if (value <= 0) return value.ToString(CultureInfo.InvariantCulture);
			return "+" + value;
		}

		public static float Polarity(this float direction)
		{
			if (direction < 0) return -1;
			if (direction > 0) return 1;
			return 0;
		}
	}
}