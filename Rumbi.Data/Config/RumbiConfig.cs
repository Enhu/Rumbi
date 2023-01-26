using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rumbi.Data.Config
{
    public class RumbiConfig
    {
        private static readonly IConfigurationRoot AppSettings
    = new ConfigurationBuilder().AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: false, reloadOnChange: true).Build();
        
        public static BotConfig Configuration { get; } = AppSettings.Get<BotConfig>();
    }
}
