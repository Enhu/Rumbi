using Discord.Interactions;
using Rumbi.Services;

namespace Rumbi.Modules
{
    public class CommandsModule : InteractionModuleBase<SocketInteractionContext>
    {
        private InteractionHandler _handler;

        public CommandsModule(InteractionHandler handler)
        {
            _handler = handler;
        }

        [SlashCommand("echo", "Repeat the input")]
        public async Task Echo(string echo, [Summary(description: "mention the user")] bool mention = false)
            => await RespondAsync(echo + (mention ? Context.User.Mention : string.Empty));

        [SlashCommand("ping", "Pings the bot and returns its latency.")]
        public async Task GreetUserAsync()
            => await RespondAsync(text: $":ping_pong: It took me {Context.Client.Latency}ms to respond to you!", ephemeral: true);

        [SlashCommand("colorme", "Add yourself a server color!.")]
        public async Task ColorUser()
        {
            throw new NotImplementedException();
        }
    }
}
