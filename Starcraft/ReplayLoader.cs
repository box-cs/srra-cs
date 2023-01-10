using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using static srra.Starcraft.Match;

namespace srra.Starcraft;

public class ReplayLoader
{
    public JObject MatchDictionary;
    public string FilePath;
    private JToken? PlayerDescs { get => MatchDictionary["Computed"]?["PlayerDescs"]; }
    private JToken? MatchPlayers { get => MatchDictionary["Header"]?["Players"]; }
    private JToken? LeaveCommands { get => MatchDictionary?["Computed"]?["LeaveGameCmds"]; }
    private string Host { get => MatchDictionary["Header"]?["Host"]?.Value<string>() ?? ""; }
    private string? ActualMapName { get => MatchDictionary?["Header"]?["Map"]?.Value<string>(); }
    private int GameTypeId { get => MatchDictionary?["Header"]?["Type"]?["ID"]?.Value<int>() ?? (int)GameType.Unkown; }
    private bool IsOfflineGame { get => string.IsNullOrEmpty(Host); }

    public ReplayLoader(string match, string filePath)
    {
        MatchDictionary = JObject.Parse(match);
        FilePath = filePath;
    }

    public Match ToMatch()
    {
        var opponent = new Player();
        var player = new Player();
        List<Player> players = ExtractPlayers(MatchPlayers, PlayerDescs);

        if (IsOfflineGame) {
            player = players?.Find(p => p.ID != 255);  
            opponent = players?.Find(p => p.ID == 255); // 255 is the Computer Player ID
        }
        else {
            var playerNames = ConfigurationManager.AppSettings["PlayerNames"]?.Split(',').ToList() ?? new();
            opponent = players?.Find(p => p?.Name is not null && !playerNames.Contains(p.Name));
            player = players?.Find(p => p.ID != opponent?.ID);
        }

        // Determining winner
        int winnerTeam = MatchDictionary?["Computed"]?["WinnerTeam"]?.Value<int>() ?? 0;
        players?.ForEach(player => player.DetermineMatchOutcomes(LeaveCommands, winnerTeam, Host));
        // Uses player names to assume who is the replay owner
        // As a replay owner, we're able to determine if we've lost (and for 1v1 games, we can also determine winner)
        // This means that for 1v1s our opponents results are the opposite of our loss result
        if (player?.HasWonMatch == opponent?.HasWonMatch && players?.Count == 2 && opponent is not null)
            opponent.HasWonMatch = !opponent?.HasWonMatch;

        var sanitizedMapName = new string(ActualMapName?.ToList()
            .FindAll(c => char.IsLetterOrDigit(c) || char.IsWhiteSpace(c) || char.IsPunctuation(c)).ToArray());
        return new Match() {
            // Meta
            FilePath = FilePath,
            Host = Host,
            Duration = TimeSpan.FromMilliseconds((MatchDictionary?["Header"]?["Frames"]?.Value<int>() ?? 0) * 42),
            Date = MatchDictionary?["Header"]?["StartTime"]?.Value<DateTime>() ?? DateTime.UnixEpoch,
            // Map Data
            Map = sanitizedMapName,
            MatchType = MatchDictionary?["Header"]?["Type"]?["Name"]?.Value<string>() ?? "Unknown",
            MatchTypeId = ((GameType)GameTypeId),
            WinnerTeam = winnerTeam,
            // Player Data
            Players = players ?? new(),
            MatchUp = $"{GetRaceAlias(player?.Race)}v{GetRaceAlias(opponent?.Race)}",
            // Players represented in datagrid columns
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
