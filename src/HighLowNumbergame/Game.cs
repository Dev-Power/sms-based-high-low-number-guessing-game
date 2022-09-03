namespace HighLowNumbergame;

public class Game
{
    public int Target { get; set; } = new Random((int)DateTime.UtcNow.Ticks).Next(Constants.MIN_NUMBER, Constants.MAX_NUMBER + 1); // Ceiling is exclusive so add 1
    public int GuessCount { get; set; } = 0;
}
