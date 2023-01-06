namespace srra
{
    public class Player
    {
        public int? ID;
        public int? TeamID;
        public string? Name;
        public int? APM;
        public int? EAPM;
        public string? Race;
        public Player(int? id, int? teamId, string? name, int? apm, int? eapm, string? race)
        {
            ID = id;
            TeamID = teamId;
            Name = name;    
            APM = apm;
            EAPM = eapm;
            Race = race; 
        }
    }
}
