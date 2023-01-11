using System;
using System.Collections.Generic;
using System.Linq;
using static srra.Starcraft.Match;

namespace srra.Starcraft
{
    public class MatchFilterBuilder
    {
        private IEnumerable<Match> matches;
        private string? name;
        private GameType? gameType;
        private string? map;
        private string? matchup;

        public MatchFilterBuilder(IEnumerable<Match> matches) 
        { 
            this.matches = matches;
        }

        public MatchFilterBuilder WithName(string? name)
        {
            this.name = name;
            return this;
        }

        public MatchFilterBuilder WithMap(string? map)
        {
            this.map = map;
            return this;
        }

        public MatchFilterBuilder WithGameType(string? gameType)
        {
            if (Enum.TryParse<GameType>(gameType, out var gameTypeFilter))
                this.gameType = gameTypeFilter;
            return this;
        }

        public MatchFilterBuilder WithMatchUp(string? matchUp)
        {
            this.matchup= matchUp;
            return this;
        }

        public IEnumerable<Match> Build()
        {
            IEnumerable<Match> filteredMatches = matches.Select(x => x);
            // Apply Player Name Filter
            if (!string.IsNullOrEmpty(name))
                filteredMatches = filteredMatches.Where(match =>
                match.Players.Any(player => player?.Name is not null && player.Name.ToLower().Contains(name.ToLower())));

            // Apply Map Name Filter
            if (!string.IsNullOrEmpty(map))
                filteredMatches = filteredMatches.Where(match => match.Map.ToLower().Contains(map.ToLower()));

            // Apply Match Type Filter
            if (gameType != null)
                filteredMatches = filteredMatches.Where(match => match.MatchTypeId == gameType);

            // Apply Match Up Filter
            if (matchup != "Any")
                filteredMatches = filteredMatches.Where(match => match.MatchUp == matchup || new string(match.MatchUp.Reverse().ToArray()) == matchup);
            return filteredMatches;
        }
    }
}
