using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Configuration;

namespace srra
{
    public class Match
    {
        public string FilePath { get; set; }
        public string? Name { get; set; }
        public string? APMString { get; set; }
        public string? OpponentName { get; set; }
        public string? OpponentAPMString { get; set; }
        public string? MatchUp { get; set; }
        public string? Map { get; set; }
        public string? Result { get; set; }
        public string? Date { get; set; }
        public string? MatchType { get; set; }
        public GameType MatchTypeId { get; set; }
        public string? Winner { get; set; }
        public List<Player> Players = new();
        public Dictionary<string, JsonElement>? MatchDictionary;
        public bool IsLadderMatch { get => MatchTypeId == GameType.TopVsBottom; }

        public Match(string match, string filePath)
        {
            MatchDictionary = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(match);
            if (MatchDictionary is null) throw new Exception("Match deserialization failed");
            FilePath = filePath;

            // Player Data
            var playerName = ConfigurationManager.AppSettings["PlayerName"];
            var matchPlayerDescs = MatchDictionary["Computed"].GetNestedJsonObject()?["PlayerDescs"];
            var matchPlayers = MatchDictionary["Header"].GetNestedJsonObject()?["Players"];
            Players = ExtractPlayers(matchPlayers, matchPlayerDescs);
            // Bug -- I tried to be fancy, by it seems like we'll have to match by name first ..
            var opponent = Players?.Find(p => p.Name != playerName);
            var player = Players?.Find(p => p.ID != opponent?.ID);
            // Match Data
            MatchUp = $"{GetRaceAlias(player?.Race)}v{GetRaceAlias(opponent?.Race)}";
            ExtractMatchData();
            int? winnerTeamID = MatchDictionary["Computed"].GetNestedJsonObject()?["WinnerTeam"].GetInt32();

            Name = $"{player?.Name} {((player?.TeamID == winnerTeamID) ? "👑" : "☠")}";
            APMString = $"{player?.APM}/{player?.EAPM}";

            OpponentName = $"{opponent?.Name} {((opponent?.TeamID == winnerTeamID) ? "👑" : "☠")}";
            OpponentAPMString = $"{opponent?.APM}/{opponent?.EAPM}";
        }

        private void ExtractMatchData()
        {
            if (MatchDictionary is null) return;
            Date = MatchDictionary["Header"].GetNestedJsonObject()?["StartTime"].ToString();
            MatchType = MatchDictionary["Header"]
                .GetNestedJsonObject()?["Type"]
                .GetNestedJsonObject()?["Name"].ToString();
            MatchTypeId = (GameType)(MatchDictionary["Header"]
                .GetNestedJsonObject()?["Type"]
                .GetNestedJsonObject()?["ID"].GetInt32() ?? (int)GameType.Unkown);
            Map = MatchDictionary["Header"].GetNestedJsonObject()?["Map"].ToString();
        }

        private static List<Player> ExtractPlayers(JsonElement? matchPlayers, JsonElement? matchPlayerDescs)
        {
            if (matchPlayerDescs == null || matchPlayers is null) return new();
            var playerName = matchPlayers.Value[0].GetNestedJsonObject()?["Name"].ToString();
            var playerId = matchPlayers.Value[0].GetNestedJsonObject()?["ID"].GetInt32();
            var playerTeamId = matchPlayers.Value[0].GetNestedJsonObject()?["Team"].GetInt32();
            var playerRace = matchPlayers.Value[0].GetNestedJsonObject()?["Race"].GetNestedJsonObject()?["Name"].ToString();
            var playerAPM = matchPlayerDescs.Value[0].GetNestedJsonObject()?["APM"].GetInt32();
            var playerEAPM = matchPlayerDescs.Value[0].GetNestedJsonObject()?["EAPM"].GetInt32();

            var opponentName = matchPlayers.Value[1].GetNestedJsonObject()?["Name"].ToString();
            var opponentId = matchPlayers.Value[1].GetNestedJsonObject()?["ID"].GetInt32();
            var opponentTeamId = matchPlayers.Value[1].GetNestedJsonObject()?["Team"].GetInt32();
            var opponentRace = matchPlayers.Value[1].GetNestedJsonObject()?["Race"].GetNestedJsonObject()?["Name"].ToString();
            var opponentAPM = matchPlayerDescs.Value[1].GetNestedJsonObject()?["APM"].GetInt32();
            var opponentEAPM = matchPlayerDescs.Value[1].GetNestedJsonObject()?["EAPM"].GetInt32();

            return new () {
                new Player(playerId, playerTeamId, playerName, playerAPM, playerEAPM, playerRace),
                new Player(opponentId, opponentTeamId, opponentName, opponentAPM, opponentEAPM, opponentRace),
            };
        }

        private static string? GetRaceAlias(string? race)
        {
            var raceAlias = new Dictionary<string, string>() {
                { "Terran" , "T"},
                { "Protoss" , "P"},
                { "Zerg" , "Z"},
            };
            if (race is not null && raceAlias.TryGetValue(race, out var value))
                return value;
            return raceAlias.Values.ToList().Find(k => k == race) ?? race;
        }

        public override string ToString()
        {
            return $"{Name} vs {OpponentName} - {MatchUp} - {Map} - {Result} - {Date}";
        }

        internal void OpenReplayFolder()
        {
            using var proc = new Process() {
                StartInfo = new ProcessStartInfo() {
                    UseShellExecute = false,
                    FileName = "explorer.exe",
                    CreateNoWindow = true,
                    Arguments = $"/e, /select, \"{FilePath}\"",
                },
            };
            proc.Start();
        }
    }
}
