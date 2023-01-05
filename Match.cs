namespace srra
{
    public class Match
    {
        public string? Name { get; set; }
        public string? APM { get; set; }
        public string? OpponentName { get; set; }
        public string? OpponentApm { get; set; }
        public string? MatchUp { get; set; }
        public string? Map { get; set; }
        public string? Result { get; set; }
        public string? Date { get; set; }
        public override string ToString()
        {
            return $"{Name}{APM}{OpponentName}{OpponentApm}{MatchUp}{Map}{Result}{Date}";
        }
    }
}
