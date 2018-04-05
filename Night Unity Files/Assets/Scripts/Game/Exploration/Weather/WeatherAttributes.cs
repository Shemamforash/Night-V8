namespace Game.World.Environment_and_Weather
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
    }
}