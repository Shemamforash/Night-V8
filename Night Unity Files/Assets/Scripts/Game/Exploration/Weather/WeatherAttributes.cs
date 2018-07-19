using System.Xml;
using SamsHelper.Libraries;

namespace Game.Exploration.Weather
{
    public class WeatherAttributes
    {
        public readonly float FogAmount, RainAmount, HailAmount, DustAmount, SunAmount;

        public WeatherAttributes(XmlNode weatherNode)
        {
            XmlNode particleNode = Helper.GetNode(weatherNode, "Particles");
            RainAmount = Helper.FloatFromNode(particleNode, "Rain");
            FogAmount = Helper.FloatFromNode(particleNode, "Fog");
            DustAmount = Helper.FloatFromNode(particleNode, "Dust");
            HailAmount = Helper.FloatFromNode(particleNode, "Hail");
            SunAmount = Helper.FloatFromNode(particleNode, "Sun");
        }
    }
}