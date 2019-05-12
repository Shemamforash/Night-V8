using System.Xml;
using Extensions;

namespace Game.Exploration.Weather
{
	public class WeatherAttributes
	{
		public readonly float FogAmount, RainAmount, HailAmount, DustAmount, SunAmount, WindAmount;

		public WeatherAttributes(XmlNode weatherNode)
		{
			weatherNode = weatherNode.SelectSingleNode("Particles");
			RainAmount  = weatherNode.ParseFloat("Rain");
			FogAmount   = weatherNode.ParseFloat("Fog");
			DustAmount  = weatherNode.ParseFloat("Dust");
			HailAmount  = weatherNode.ParseFloat("Hail");
			SunAmount   = weatherNode.ParseFloat("Sun");
			WindAmount  = weatherNode.ParseFloat("Wind");
		}
	}
}