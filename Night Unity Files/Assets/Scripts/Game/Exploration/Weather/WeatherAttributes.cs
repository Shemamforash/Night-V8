using System.Xml;

namespace Game.Exploration.Weather
{
    public class WeatherAttributes
    {
        public readonly float FogAmount, RainAmount, HailAmount, DustAmount, SunAmount;

        public WeatherAttributes(float rain, float fog, float dust, float hail, float sun)
        {
            RainAmount = rain;
            FogAmount = fog;
            DustAmount = dust;
            HailAmount = hail;
            SunAmount = sun;
        }

        public WeatherAttributes(XmlNode weatherNode)
        {
            XmlNode particleNode = weatherNode.SelectSingleNode("Particles");
            RainAmount = float.Parse(particleNode.SelectSingleNode("Rain").InnerText);
            FogAmount = float.Parse(particleNode.SelectSingleNode("Fog").InnerText);
            DustAmount = float.Parse(particleNode.SelectSingleNode("Dust").InnerText);
            HailAmount = float.Parse(particleNode.SelectSingleNode("Hail").InnerText);
            SunAmount = float.Parse(particleNode.SelectSingleNode("Sun").InnerText);
        }
    }
}