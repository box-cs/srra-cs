using System.Collections;
using System.Collections.Generic;

namespace srra.Starcraft
{
    public class WinRates
    {
        public List<SimpleWinRate> ToSimpleWinRates()
        {
            List<SimpleWinRate> simpleWinRates = new ();
            var races = new List<string>() { "Terran", "Protoss", "Zerg"};
            foreach (var playerRace in races) { 
              foreach (var opponentRace in races) {
                    simpleWinRates.Add(new SimpleWinRate(this, playerRace, opponentRace));
                }
            }
            return simpleWinRates;
        }

        public readonly Dictionary<string, Matchup> _winRates = new() {
            {"Terran", new Matchup() },
            {"Protoss", new Matchup() },
            {"Zerg", new Matchup() },
        };
        public Matchup this[string race] { get => _winRates[race]; set => _winRates[race] = value; }
    }

    public class MatchResults
    {
        private readonly Dictionary<string, int> results = new() {
            { "Wins", 0 },
            { "Games", 0 }
        };
        public MatchResults() { }
        public int this[string key] { get => results[key]; set => results[key] = value; }
        public ICollection Keys => results.Keys;
        public ICollection Values => results.Values;
        public bool Contains(string key) => results.ContainsKey(key);
        public double GetWinRate() => (double)this["Wins"] / this["Games"];
    }

    public class Matchup
    {
        readonly Dictionary<string, MatchResults> matchup = new() {
            {"Terran", new MatchResults() },
            {"Protoss", new MatchResults() },
            {"Zerg", new MatchResults() },

        };
        public MatchResults this[string opponentRace] { get => matchup[opponentRace]; set => matchup[opponentRace] = value; }
    }
}
