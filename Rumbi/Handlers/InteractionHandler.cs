using System.Runtime.CompilerServices;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Rumbi.Data.Config;
using System.Reflection;

namespace Rumbi.Services
{
    public class InteractionHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly InteractionService _handler;
        private readonly IServiceProvider _services;
        private readonly AppConfig _config;

        public InteractionHandler(
            DiscordSocketClient client,
            InteractionService handler,
            IServiceProvider services,
            AppConfig config
        )
        {
            _client = client;
            _handler = handler;
            _services = services;
            _config = config;
        }

        public async Task InitializeAsync()
        {
            _client.Ready += ReadyAsync;
            _handler.Log += LogAsync;

            await _handler.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

            _client.InteractionCreated += HandleInteraction;
            _handler.InteractionExecuted += HandleInteractionExecutedAsync;
        }

        private async Task LogAsync(LogMessage log) => Console.WriteLine(log);

        private async Task ReadyAsync()
        {
            await _handler.RegisterCommandsToGuildAsync(_config.Guild, true);
        }

        private async Task HandleInteraction(SocketInteraction interaction)
        {
            try
            {
                var context = new SocketInteractionContext(_client, interaction);
                await _handler.ExecuteCommandAsync(context, _services);
            }
            catch
            {
                if (interaction.Type is InteractionType.ApplicationCommand)
                    await interaction
                        .GetOriginalResponseAsync()
                        .ContinueWith(async (msg) => await msg.Result.DeleteAsync());
            }
        }

        private async Task HandleInteractionExecutedAsync(
            ICommandInfo command,
            IInteractionContext context,
            IResult result
        )
        {
            if (!result.IsSuccess)
            {
                switch (result.Error)
                {
                    case InteractionCommandError.UnmetPrecondition:
                        //implement in the future
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
