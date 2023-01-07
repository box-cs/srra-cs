using System.Configuration;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace srra.Starcraft
{
    public class Player
    {
        public int? ID;
        public int? TeamID;
        public string? Name;
        public int? APM;
        public int? EAPM;
        public string? Race;
        public bool? HasWonMatch;

        public bool DidPlayerLeaveGame(JToken? leaveCommands)
        {
            var playerLeftGame = false;
            leaveCommands?.ToList().ForEach(leavers =>
            {
                var leaverId = leavers?["PlayerID"]?.Value<int>();
                if (ID is not null)
                    playerLeftGame = leaverId == ID;
            });
            return playerLeftGame;
        }


        public void DetermineMatchOutcomes(JToken? leaveCommands, Match match)
        {
            if (string.IsNullOrEmpty(match.Host))
            {
                HasWonMatch = null; // There's no way to determine winner on single player matches
            }
            else
            {
                // We can determine the losers
                if (DidPlayerLeaveGame(leaveCommands) || match.WinnerTeam == 0)
                    HasWonMatch = false;
                // We can determine the winner
                if (match.WinnerTeam != 0)
                {
                    HasWonMatch = TeamID == match.WinnerTeam;
                    return;
                }

                // We cannot determine the winner
                if (leaveCommands is null)
                {
                    if (Name == ConfigurationManager.AppSettings["PlayerName"])
                        HasWonMatch = null;
                    return;
                }
            }
        }

    }
}
