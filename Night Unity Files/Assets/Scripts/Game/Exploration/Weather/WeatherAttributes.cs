using System.Xml;
using SamsHelper.Libraries;

namespace Game.Exploration.Weather
{
    public class WeatherAttributes
    {
        public readonly float FogAmount, RainAmount, HailAmount, DustAmount, SunAmount, WindAmount;

        public WeatherAttributes(XmlNode weatherNode)
        {
            RainAmount = Helper.FloatFromNode(weatherNode, "Rain");
            FogAmount = Helper.FloatFromNode(weatherNode, "Fog");
            DustAmount = Helper.FloatFromNode(weatherNode, "Dust");
            HailAmount = Helper.FloatFromNode(weatherNode, "Hail");
            SunAmount = Helper.FloatFromNode(weatherNode, "Sun");
            WindAmount = Helper.FloatFromNode(weatherNode, "Wind");
        }
    }
}