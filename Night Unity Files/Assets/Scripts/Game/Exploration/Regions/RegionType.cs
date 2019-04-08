namespace Game.Exploration.Regions
{
    public enum RegionType
    {
        None,

        //Non Dynamic
        Gate,
        Tomb,
        Rite,
        Tutorial,

        //Dynamic
        Shelter, //5
        Temple, //6
        Animal,
        Danger,
        Fountain,
        Shrine,
        Monument,
        Cache
    }
}