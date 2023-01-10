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
            leaveCommands?.ToList().ForEach(leavers => {
                var leaverId = leavers?["PlayerID"]?.Value<int>();
                if (ID is not null)
                    playerLeftGame = leaverId == ID;
            });
            return playerLeftGame;
        }

        public void DetermineMatchOutcomes(JToken? leaveCommands, int winnerTeam, string host)
        {
            if (string.IsNullOrEmpty(host)) {
                HasWonMatch = null; // There's no way to determine winner on single player matches
            }
            else {
                // We can determine the losers
                if (DidPlayerLeaveGame(leaveCommands) || winnerTeam == 0)
                    HasWonMatch = false;
                // We can determine the winner
                if (winnerTeam != 0) {
                    HasWonMatch = TeamID == winnerTeam;
                    return;
                }

                // We cannot determine the winner
                if (leaveCommands is null) {
                    if ((ConfigurationManager.AppSettings["PlayerNames"] ?? "").Split(',').Contains(Name))
                        HasWonMatch = null;
                    return;
                }
            }
        }
    }
}
