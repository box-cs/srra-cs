using System;
using System.Collections.Generic;
using System.Text.Json;

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
        // NOT USED YET
        public string? MatchType { get; set; }
        public int? MatchTypeId { get; set; }
        public int? WinnerTeam { get; set; }
        public Dictionary<string, JsonElement>? MatchDictionary;

        public Match(string match)
        {
            MatchDictionary = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(match);
            if (MatchDictionary is null) throw new Exception("Match deserialization failed");

            Date = MatchDictionary["Header"].GetNestedJsonObject()?["StartTime"].ToString();
            MatchType = MatchDictionary["Header"]
                .GetNestedJsonObject()?["Type"]
                .GetNestedJsonObject()?["Name"].ToString();
            MatchTypeId = MatchDictionary["Header"]
                .GetNestedJsonObject()?["Type"]
                .GetNestedJsonObject()?["ID"].GetInt32();
            Map = MatchDictionary["Header"].GetNestedJsonObject()?["Map"].ToString();
            WinnerTeam = MatchDictionary["Computed"].GetNestedJsonObject()?["WinnerTeam"].GetInt32();
            var matchPlayers = MatchDictionary["Header"].GetNestedJsonObject()?["Players"];
            var matchPlayerDescs = MatchDictionary["Computed"].GetNestedJsonObject()?["PlayerDescs"];
        }

        public Match()
        {

        }

        public override string ToString()
        {
            return $"{Name}{APM}{OpponentName}{OpponentApm}{MatchUp}{Map}{Result}{Date}";
        }

        internal void PrintMatch()
        {
            if (MatchDictionary == null) return;

            System.Diagnostics.Trace.WriteLine(Date);
        }
    }
}
