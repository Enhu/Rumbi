using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rumbi.Behaviors;
using Rumbi.Services;
using Serilog;
using Serilog.Events;

public class Program
{
    private readonly IConfiguration _configuration;

    private readonly IServiceProvider _services;

    private readonly DiscordSocketConfig _socketConfig = new()
    {
        GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.GuildMembers | GatewayIntents.GuildPresences,
        AlwaysDownloadUsers = true,
    };
    public Program()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File("log.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

        Log.Information($"Running environment: {environmentName}");

        _configuration = new ConfigurationBuilder()
            .AddJsonFile($"appsettings.{environmentName}.json", optional: false, reloadOnChange: true)
            .Build();

        _services = new ServiceCollection()
                .AddSingleton(_configuration)
                .AddSingleton(_socketConfig)
                .AddSingleton<DiscordSocketClient>()
                .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()))
                .AddSingleton<InteractionHandler>()
                .AddSingleton<UserBehavior>()
                .BuildServiceProvider();
    }

    static void Main(string[] args)
        => new Program().RunAsync()
            .GetAwaiter()
            .GetResult();

    public async Task RunAsync()
    {
        var client = _services.GetRequiredService<DiscordSocketClient>();
        var interaction = _services.GetRequiredService<InteractionService>();

        // Here we can initialize the service that will register and execute our commands
        await _services.GetRequiredService<InteractionHandler>()
            .InitializeAsync();

        _services.GetRequiredService<UserBehavior>()
            .Initialize();

        client.Log += LogAsync;
        interaction.Log += LogAsync;

        // Bot token can be provided from the Configuration object we set up earlier
        await client.LoginAsync(TokenType.Bot, _configuration["Token"]);
        await client.StartAsync();

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

        Log.Write(severity, message.Exception, "[{Source}] {Message}", message.Source, message.Message);

        return Task.CompletedTask;
    }

}
