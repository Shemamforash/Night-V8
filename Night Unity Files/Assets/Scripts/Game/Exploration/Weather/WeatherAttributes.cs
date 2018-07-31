using System.Xml;
using SamsHelper.Libraries;

namespace Game.Exploration.Weather
{
    public class WeatherAttributes
    {
        public readonly float FogAmount, RainAmount, HailAmount, DustAmount, SunAmount, WindAmount;

        public WeatherAttributes(XmlNode weatherNode)
        {
            RainAmount = weatherNode.FloatFromNode("Rain");
            FogAmount = weatherNode.FloatFromNode("Fog");
            DustAmount = weatherNode.FloatFromNode("Dust");
            HailAmount = weatherNode.FloatFromNode("Hail");
            SunAmount = weatherNode.FloatFromNode("Sun");
            WindAmount = weatherNode.FloatFromNode("Wind");
        }
    }
}