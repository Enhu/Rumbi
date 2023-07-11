using DotNetEnv;

namespace Rumbi.Data.Config
{
    public class AppConfig
    {
        public string Token { get; set; } = string.Empty;

        public string ConnectionString { get; set; } = string.Empty;
        public string TwitchClientId { get; set; } = string.Empty;
        public string TwitchSecret { get; set; } = string.Empty;

        public ulong Guild { get; set; } = 0;
        public string Version { get; set; } = string.Empty;

        public RoleConfig RoleConfig { get; set; }
        public ChannelConfig ChannelConfig { get; set; }

        public AppConfig()
        {
#if DEBUG
            var currentDirectory = Directory.GetCurrentDirectory();
            var envPath = Path.Combine(currentDirectory, ".env");

            //TODO: Fix this??? Uncomment this lines to use dotnet ef
            //var envPath = Path.Combine("..", "Rumbi", ".env");
            //DotNetEnv.Env.Load(envPath);
            Env.Load(envPath);
#endif

            string dbServer = Environment.GetEnvironmentVariable("DB_SERVER");
            string dbPort = Environment.GetEnvironmentVariable("DB_PORT");
            string dbUserId = Environment.GetEnvironmentVariable("DB_USERID");
            string dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD");
            string database = Environment.GetEnvironmentVariable("DB_NAME");

            ConnectionString = string.Format(
                "Server={0};Port={1};Database={4};User Id={2};Password={3}",
                dbServer,
                dbPort,
                dbUserId,
                dbPassword,
                database
            );

            Token = Environment.GetEnvironmentVariable("TOKEN");
            Version = Environment.GetEnvironmentVariable("VERSION");
            Guild = ulong.Parse(Environment.GetEnvironmentVariable("GUILD"));
            TwitchClientId = Environment.GetEnvironmentVariable("TWITCH_CLIENTID");
            TwitchSecret = Environment.GetEnvironmentVariable("TWITCH_SECRET");

            RoleConfig = new()
            {
                Runner = ulong.Parse(Environment.GetEnvironmentVariable("RUNNER_ROLE")),
                Streaming = ulong.Parse(Environment.GetEnvironmentVariable("STREAMING_ROLE")),
                Verifier = ulong.Parse(Environment.GetEnvironmentVariable("VERIFIER_ROLE")),
                SRDCMod = ulong.Parse(Environment.GetEnvironmentVariable("SRDCMOD_ROLE")),
                Admin = ulong.Parse(Environment.GetEnvironmentVariable("ADMIN_ROLE"))
            };

            ChannelConfig = new()
            {
                LbAnnouncements = ulong.Parse(Environment.GetEnvironmentVariable("LBANN_CH")),
                LbVotes = ulong.Parse(Environment.GetEnvironmentVariable("LBVOTES_CH")),
                Bot = ulong.Parse(Environment.GetEnvironmentVariable("BOT_CH")),
                Logs = ulong.Parse(Environment.GetEnvironmentVariable("LOG_CH")),
                Runner = ulong.Parse(Environment.GetEnvironmentVariable("RUNNER_CH")),
            };
        }
    }

    public class RoleConfig
    {
        public ulong Streaming { get; set; } = 0;
        public ulong Runner { get; set; } = 0;
        public ulong Verifier { get; set; } = 0;
        public ulong SRDCMod { get; set; } = 0;
        public ulong Admin { get; set; } = 0;
    }

    public class ChannelConfig
    {
        public ulong LbAnnouncements { get; set; } = 0;

        public ulong LbVotes { get; set; } = 0;

        public ulong Bot { get; set; } = 0;

        public ulong Logs { get; set; } = 0;

        public ulong Runner { get; set; } = 0;
    }
}