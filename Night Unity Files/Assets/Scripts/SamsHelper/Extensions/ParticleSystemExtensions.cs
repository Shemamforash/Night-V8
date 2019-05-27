using UnityEngine;

namespace Extensions
{
	public static class ParticleSystemExtensions
	{
		public static void SetStartColour(this ParticleSystem ps, Color colour)
		{
			ParticleSystem.MainModule main = ps.main;
			main.startColor = colour;
		}

		public static void SetEmissionRateOverTime(this ParticleSystem ps, int emissionRate)
		{
			ParticleSystem.EmissionModule emissionModule = ps.emission;
			emissionModule.rateOverTime = emissionRate;
		}

		public static void SetEmissionRateOverDistance(this ParticleSystem ps, int emissionRate)
		{
			ParticleSystem.EmissionModule emissionModule = ps.emission;
			emissionModule.rateOverDistance = emissionRate;
		}

		public static void SetEmissionRateOverDistance(this ParticleSystem ps, int emissionRateMin, int emissionRateMax)
		{
			ParticleSystem.EmissionModule emissionModule = ps.emission;
			emissionModule.rateOverDistance = new ParticleSystem.MinMaxCurve(emissionRateMin, emissionRateMax);
		}

		public static void SetColourOverLifetime(this ParticleSystem ps, Color colourMin, Color colourMax)
		{
			ParticleSystem.ColorOverLifetimeModule colorModule = ps.colorOverLifetime;
			colorModule.color = new ParticleSystem.MinMaxGradient(colourMin, colourMax);
		}
	}
}