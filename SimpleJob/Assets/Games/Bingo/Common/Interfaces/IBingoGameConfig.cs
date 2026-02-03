namespace Bingo.Interfaces
{
    public interface IBingoGameConfig
    {
        int CardSize { get; }
        float CallNumberDelay { get; }
        int MaxPlayers { get; }
    }
}
