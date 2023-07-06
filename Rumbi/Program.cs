using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Rumbi.Behaviors;
using Rumbi.Data;
using Rumbi.Data.Config;
using Rumbi.Services;
using Serilog;
using Serilog.Events;

public class Program
{
    private readonly IServiceProvider _services;

    private readonly AppConfig config = new();

    private readonly DiscordSocketConfig _socketConfig =
        new()
        {
            GatewayIntents =
                GatewayIntents.AllUnprivileged
                | GatewayIntents.GuildMembers
                | GatewayIntents.GuildPresences
                | GatewayIntents.MessageContent
                | GatewayIntents.Guilds,
            AlwaysDownloadUsers = true,
        };

    public Program()
    {
        Log.Logger = new LoggerConfiguration().MinimumLevel
            .Verbose()
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateLogger();

        Log.Information("Loading services...");

        _services = new ServiceCollection()
            .AddSingleton<AppConfig>()
            .AddDbContext<RumbiContext>(options => options.UseNpgsql(config.ConnectionString))
            .AddSingleton(_socketConfig)
            .AddSingleton<DiscordSocketClient>()
            .AddSingleton<InteractionService>()
            .AddSingleton<InteractionHandler>()
            .AddSingleton<TwitchService>()
            .AddSingleton<UserBehavior>()
            .AddSingleton<MemeBehavior>()
            .BuildServiceProvider();

        Log.Information("All services loaded.");
    }

    static void Main(string[] args) => new Program().RunAsync().GetAwaiter().GetResult();

    public async Task RunAsync()
    {
        var client = _services.GetRequiredService<DiscordSocketClient>();

        await _services.GetRequiredService<InteractionHandler>().InitializeAsync();
        var interactionService = _services.GetService<InteractionService>();

        _services.GetRequiredService<UserBehavior>().Initialize();

        _services.GetRequiredService<MemeBehavior>().Initialize();

        client.Log += LogAsync;

        Log.Information("Logging in...");
        try
        {
            await client.LoginAsync(TokenType.Bot, config.Token);
            await client.StartAsync();
        }
        catch (Exception e)
        {
            Log.Error("An error ocurred trying to log in.");
            Log.Error(e, e.Message, e.InnerException);
            Environment.Exit(1);
        }
        Log.Information("Successfully logged in.");

        // Never quit the program until manually forced to.
        await Task.Delay(Timeout.Infinite);
    }

    private static Task LogAsync(LogMessage message)
    {
        var severity = message.Severity switch
        {
            LogSeverity.Critical => LogEventLevel.Fatal,
            LogSeverity.Error => LogEventLevel.Error,
            LogSeverity.Warning => LogEventLevel.Warning,
            LogSeverity.Info => LogEventLevel.Information,
            LogSeverity.Verbose => LogEventLevel.Verbose,
            LogSeverity.Debug => LogEventLevel.Debug,
            _ => LogEventLevel.Information
        };

        Log.Write(
            severity,
            message.Exception,
            "[{Source}] {Message}",
            message.Source,
            message.Message
        );

        return Task.CompletedTask;
    }
}
