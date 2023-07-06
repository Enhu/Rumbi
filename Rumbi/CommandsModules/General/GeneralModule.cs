using Discord.Interactions;
using Rumbi.Data;
using Rumbi.Data.Config;
using Rumbi.Data.Models;
using Serilog;
using System.Drawing;

namespace Rumbi.Modules.General
{
    public class GeneralModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly RumbiContext _dbContext;
        private readonly AppConfig _config;

        public GeneralModule(RumbiContext context, AppConfig config)
        {
            _dbContext = context;
            _config = config;
        }

        [SlashCommand("ping", "Pings the bot and returns its latency.")]
        public async Task GreetUserAsync() =>
            await RespondAsync(
                text: $":ping_pong: It took me {Context.Client.Latency}ms to respond to you!",
                ephemeral: true
            );

        [SlashCommand("about", "Information about this bot.")]
        public async Task About() => await RespondAsync(text: "Nya :3", ephemeral: true);
    }
}
