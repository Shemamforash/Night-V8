namespace Game.World
{
    public class EnvironmentRegion : Inventory

    {
        private string _name, _type, _regionDescription;
//        private List<Encounter> _enemyEncounters = new List<Encounter>();

        public EnvironmentRegion(string name, string type, string description)
        {
            _name = name;
            _type = type;
            _regionDescription = description;
        }
    }
}