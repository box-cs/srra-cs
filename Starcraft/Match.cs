using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Configuration;
using Newtonsoft.Json.Linq;
using System.Text;
using System.IO;

namespace srra.Starcraft
{
    public class Match
    {
        public enum GameType
        {
            None,
            Custom,
            Melee,
            FreeForAll,
            OneOnOne,
            CaptureTheFlag,
            Greed,
            Slaughter,
            SuddenDeath,
            Ladder,
            UMS,
            TeamMelee,
            TeamFFA,
            TeamCTF,
            Unkown,
            TopVsBottom,

        };
        public string FilePath { get; set; }
        public TimeSpan Duration { get; set; }
        public string? MatchLength { get => Duration.ToString()[..8]; }
        public string? Host { get; set; }
        public string? Name { get; set; }
        public string? APMString { get; set; }
        public string? OpponentName { get; set; }
        public string? OpponentAPMString { get; set; }
        public string? MatchUp { get; set; }
        public string? Map { get; set; }
        public string? Result { get; set; }
        public DateTime? Date { get; set; }
        public string? MatchType { get; set; }
        public GameType MatchTypeId { get; set; }
        public int? WinnerTeam { get; set; }
        public List<Player> Players = new();
        public JObject? MatchDictionary;
        public bool IsLadderMatch { get => MatchTypeId == GameType.TopVsBottom; }
        // ReplayLoader should probably parse this instead of the match 
        public Match(string match, string filePath)
        {
            MatchDictionary = JObject.Parse(match);

            if (MatchDictionary is null) throw new Exception("Match deserialization failed");
            FilePath = filePath;
            Host = MatchDictionary["Header"]?["Host"]?.ToString();
            var durationInMs = (MatchDictionary["Header"]?["Frames"]?.Value<int>() ?? 0) * 42;
            Duration = TimeSpan.FromMilliseconds(durationInMs);
            // Player Data
            var matchPlayerDescs = MatchDictionary["Computed"]?["PlayerDescs"];
            var matchPlayers = MatchDictionary["Header"]?["Players"];
            Players = ExtractPlayers(matchPlayers, matchPlayerDescs);
            var opponent = new Player();
            var player = new Player();

            if (string.IsNullOrEmpty(Host))
            {
                // Represents an offline game, ID 255 represents a computer player
                player = Players?.Find(p => p.ID != 255);
                opponent = Players?.Find(p => p.ID == 255);
            }
            else
            {
                // Represents an online game
                var playerNames = ConfigurationManager.AppSettings["PlayerNames"]?.Split(',') ?? Array.Empty<string>();
                opponent = Players?.Find(p => !playerNames.Contains(p.Name));
                player = Players?.Find(p => p.ID != opponent?.ID);
            }
            // Match Data
            MatchUp = $"{GetRaceAlias(player?.Race)}v{GetRaceAlias(opponent?.Race)}";
            ExtractMatchData();
            // Determining winner
            WinnerTeam = MatchDictionary?["Computed"]?["WinnerTeam"]?.Value<int>();
            var leaveCommands = MatchDictionary?["Computed"]?["LeaveGameCmds"];
            Players?.ForEach(player => player.DetermineMatchOutcomes(leaveCommands, this));

            // This logic only works because we assume that we are the replay owners
            // As a replay owner, we're able to determine if we've won or not
            // This means that our opponent's result is the opposite (true for 1v1 games)
            if (player?.HasWonMatch == opponent?.HasWonMatch && Players.Count == 2)
            {
                opponent.HasWonMatch = !opponent?.HasWonMatch;
            }

            Name = StringifyMatchOutcome(player);
            OpponentName = StringifyMatchOutcome(opponent);
            APMString = $"{player?.APM}/{player?.EAPM}";
            OpponentAPMString = $"{opponent?.APM}/{opponent?.EAPM}";
        }

        public override string ToString() => $"{FilePath}";

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (obj is not Match objectAsMatch) return false;
            return Equals(objectAsMatch);
        }

        public override int GetHashCode()=>FilePath.GetHashCode();

        public bool Equals(Match other)
        {
            if (other == null) return false;
            return (FilePath.Equals(other.FilePath));
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

        private void ExtractMatchData()
        {
            if (MatchDictionary is null) return;
            Date = MatchDictionary["Header"]?["StartTime"]?.Value<DateTime>();
            MatchType = MatchDictionary["Header"]?["Type"]?["Name"]?.Value<string>();
            MatchTypeId = (GameType)((MatchDictionary["Header"]?["Type"]?["ID"]?.Value<int>()) ?? (int)GameType.Unkown); // Horrible
            var map = MatchDictionary["Header"]?["Map"]?.Value<string>();
            Map = new string(map?.ToList().FindAll(c => char.IsLetterOrDigit(c) || char.IsWhiteSpace(c) || char.IsPunctuation(c)).ToArray());
        }

        private static List<Player> ExtractPlayers(JToken? matchPlayers, JToken? matchPlayerDescs)
        {
            if (matchPlayerDescs == null || matchPlayers is null) return new();
            var count = matchPlayers.ToArray().Length;
            return Enumerable.Range(0, count).ToList().Select(index =>
            {
                return new Player()
                {
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

        public void OpenReplayFolder()
        {
            using var proc = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    UseShellExecute = false,
                    FileName = "explorer.exe",
                    CreateNoWindow = true,
                    Arguments = $"/e, /select, \"{FilePath}\"",
                },
            };
            proc.Start();
        }

        public void DeleteReplayFile(out bool success)
        {
            try {
                if (File.Exists(FilePath))
                    File.Delete(FilePath);
                success = true;
            }
            catch (IOException) {
                success = false;
            }
        }
    }
}
