namespace srra.Starcraft;

public class SimpleWinRate
{
    public string? MatchUp { get; set; }
    public string? WinRate { get; set; }
    public string? MatchCount { get; set; }
    public string? MatchWins { get; set; }
    public SimpleWinRate(WinRates winRates, string playerRace, string opponentRace)
    {
        WinRate = $"{winRates[playerRace][opponentRace].GetWinRate():P}";
        MatchCount = winRates[playerRace][opponentRace]["Games"].ToString();
        MatchWins = winRates[playerRace][opponentRace]["Wins"].ToString();
        MatchUp = $"{playerRace[..1]}v{opponentRace[..1]}";
    }
    public override string ToString() => $"{MatchUp}, {WinRate:P}, {MatchCount}, {MatchWins}";
}
