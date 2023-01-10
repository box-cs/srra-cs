using Newtonsoft.Json.Linq;
using ScottPlot.Drawing.Colormaps;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static srra.Starcraft.Match;
using System.Xml.Linq;

namespace srra.Starcraft;

public class ReplayLoader
{
    public JObject MatchDictionary;
    public string FilePath;
    public ReplayLoader(string match, string filePath)
    {
        MatchDictionary = JObject.Parse(match);
        FilePath = filePath;
    }

    public Match ToMatch()
    {
        var durationInMs = (MatchDictionary["Header"]?["Frames"]?.Value<int>() ?? 0) * 42;
        var matchPlayerDescs = MatchDictionary["Computed"]?["PlayerDescs"];
        var matchPlayers = MatchDictionary["Header"]?["Players"];
        var opponent = new Player();
        var player = new Player();
        List<Player> players = ExtractPlayers(matchPlayers, matchPlayerDescs);
        var host = MatchDictionary["Header"]?["Host"]?.ToString() ?? "";
        var leaveCommands = MatchDictionary?["Computed"]?["LeaveGameCmds"];

        if (string.IsNullOrEmpty(host)) {
            // Represents an offline game, ID 255 represents a computer player
            player = players?.Find(p => p.ID != 255);
            opponent = players?.Find(p => p.ID == 255);
        }
        else {
            // Represents an online game
            var playerNames = ConfigurationManager.AppSettings["PlayerNames"]?.Split(',').ToList() ?? new();
            opponent = players?.Find(p => !playerNames.Contains(p.Name));
            player = players?.Find(p => p.ID != opponent?.ID);
        }

        var map = MatchDictionary?["Header"]?["Map"]?.Value<string>();
        // Determining winner
        int? winnerTeam = MatchDictionary?["Computed"]?["WinnerTeam"]?.Value<int>();
        players?.ForEach(player => player.DetermineMatchOutcomes(leaveCommands, winnerTeam ?? 0, host));
        // This logic only works because we assume that we are the replay owners
        // As a replay owner, we're able to determine if we've won or not
        // This means that our opponent's result is the opposite (true for 1v1 games)
        if (player?.HasWonMatch == opponent?.HasWonMatch && players?.Count == 2 && opponent is not null)
            opponent.HasWonMatch = !opponent?.HasWonMatch;

        return new Match(FilePath) {
            Host = host,
            Duration = TimeSpan.FromMilliseconds(durationInMs),
            Players = players,
            Date = MatchDictionary?["Header"]?["StartTime"]?.Value<DateTime>(),
            MatchType = MatchDictionary?["Header"]?["Type"]?["Name"]?.Value<string>(),
            MatchTypeId = (GameType)((MatchDictionary?["Header"]?["Type"]?["ID"]?.Value<int>()) ?? (int)GameType.Unkown), // Horrible
            MatchUp = $"{GetRaceAlias(player?.Race)}v{GetRaceAlias(opponent?.Race)}",
            Map = new string(map?.ToList().FindAll(c => char.IsLetterOrDigit(c) || char.IsWhiteSpace(c) || char.IsPunctuation(c)).ToArray()),
            Name = StringifyMatchOutcome(player),
            OpponentName = StringifyMatchOutcome(opponent),
            APMString = $"{player?.APM}/{player?.EAPM}",
            OpponentAPMString = $"{opponent?.APM}/{opponent?.EAPM}",
        };
    }

    private static string StringifyMatchOutcome(Player? player)
    {
        var outcome = new StringBuilder();
        outcome.Append($"{player?.Name} ({player?.TeamID})");
        if (player is null || player?.HasWonMatch is null) return outcome.ToString();

        if (player.HasWonMatch == true)
            outcome.Append(" 👑");
        else if (player.HasWonMatch == false)
            outcome.Append(" ☠️");

        return outcome.ToString();
    }

    private static List<Player> ExtractPlayers(JToken? matchPlayers, JToken? matchPlayerDescs)
    {
        if (matchPlayerDescs == null || matchPlayers is null) return new();
        var count = matchPlayers.ToArray().Length;
        return Enumerable.Range(0, count).ToList().Select(index => {
            return new Player() {
                Name = matchPlayers[index]?["Name"]?.Value<string>(),
                ID = matchPlayers[index]?["ID"]?.Value<int>(),
                TeamID = matchPlayers[index]?["Team"]?.Value<int>(),
                Race = matchPlayers[index]?["Race"]?["Name"]?.Value<string>(),
                APM = matchPlayerDescs[index]?["APM"]?.Value<int>(),
                EAPM = matchPlayerDescs[index]?["EAPM"]?.Value<int>(),
                HasWonMatch = null
            };
        }).ToList();
    }

    public static string GetRaceAlias(string? race) => race?[..1] ?? "";
}
