using System;
using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
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
        public string FilePath { get; set; } = string.Empty;
        public TimeSpan Duration { get; set; } = TimeSpan.Zero;
        public string MatchLength { get => Duration.ToString()[..8]; }
        public string Host { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string APMString { get; set; } = string.Empty;
        public string OpponentName { get; set; } = string.Empty;
        public string OpponentAPMString { get; set; } = string.Empty;
        public string MatchUp { get; set; } = string.Empty;
        public string Map { get; set; } = string.Empty;
        public string Result { get; set; } = string.Empty;
        public DateTime Date { get; set; } = DateTime.UnixEpoch;
        public string MatchType { get; set; } = string.Empty;
        public GameType MatchTypeId { get; set; } = GameType.Unkown;
        public int WinnerTeam { get; set; } = 0;
        public List<Player> Players = new();
        public JObject? MatchDictionary;

        public override string ToString() => $"{FilePath}";

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (obj is not Match objectAsMatch) return false;
            return Equals(objectAsMatch);
        }

        public override int GetHashCode() => FilePath.GetHashCode();

        public bool Equals(Match other)
        {
            if (other == null) return false;
            return (FilePath.Equals(other.FilePath));
        }

        public void OpenReplayFolder()
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
