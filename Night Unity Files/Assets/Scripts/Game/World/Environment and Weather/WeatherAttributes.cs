namespace Game.World.Environment_and_Weather
{
    public class WeatherAttributes
    {
        public float FogAmount, RainAmount, HailAmount, DustAmount;

        public WeatherAttributes(float rain, float fog, float dust, float hail)
        {
            RainAmount = rain;
            FogAmount = fog;
            DustAmount = dust;
            HailAmount = hail;
        }
    }
}