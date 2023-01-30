using Microsoft.Extensions.Configuration;

namespace Rumbi.Data.Config
{
    public class RumbiConfig
    {
        private static readonly IConfigurationRoot AppSettings
    = new ConfigurationBuilder().AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: false, reloadOnChange: true).Build();

        public static BotConfig Config { get; } = AppSettings.Get<BotConfig>();

        public static RoleConfig RoleConfig { get; } = AppSettings.GetSection("Roles").Get<RoleConfig>();
        public static ChannelConfig ChannelConfig { get; } = AppSettings.GetSection("Channels").Get<ChannelConfig>();
    }
}
