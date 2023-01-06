using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace srra
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

    public class MatchLoader
    {
        public static async Task<List<string>> LoadMatches()
        {
            return await Task.Run(() => {
                var replayPath = ConfigurationManager.AppSettings["Replay_Path"];
                var screpPath = ConfigurationManager.AppSettings["SCREP_Path"];

                if (screpPath is null || replayPath is null) return new();
                var matches = Directory.GetFiles(replayPath, "*.rep", SearchOption.TopDirectoryOnly).ToList();
                matches.Reverse();
                return matches;
            });
        }
    }
}
