namespace Rumbi.Data.Config
{
    public class BotConfig
    {
        public string Token { get; init; } = null!;

        public string ConnectionString { get; init; } = null!;
        public string TwitchClientId { get; init; } = null!;
        public string TwitchSecret { get; init; } = null!;

        public ulong Guild { get; init; }
        public ulong LoggingChannel { get; init; }
        public ulong MemeChannel { get; init; }
        public ulong Streaming { get; init; }
        public string Version { get; init; } = null!;

    }
}
