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

    public class RoleConfig
    {
        public ulong Streaming { get; init; }
        public ulong Runner { get; init; }
    }

    public class ChannelConfig
    {
        public ulong LbAnnouncements { get; init; }

        public ulong LbVotes { get; init; }

        public ulong BotTalk { get; init; }

        public ulong BotLogs { get; init; }

        public ulong RunnerRole { get; init; }
    }
}
