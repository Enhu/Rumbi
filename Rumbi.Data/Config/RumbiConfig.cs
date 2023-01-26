using Microsoft.Extensions.Configuration;

namespace Rumbi.Data.Config
{
    public class RumbiConfig
    {
        private static readonly IConfigurationRoot AppSettings
    = new ConfigurationBuilder().AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: false, reloadOnChange: true).Build();

        public static BotConfig Configuration { get; } = AppSettings.Get<BotConfig>();
    }
}
