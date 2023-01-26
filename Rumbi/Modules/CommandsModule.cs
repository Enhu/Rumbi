using Discord.Interactions;
using Rumbi.Services;
using Serilog;
using System.Drawing;

namespace Rumbi.Modules
{
    public class CommandsModule : InteractionModuleBase<SocketInteractionContext>
    {
        private InteractionHandler _handler;

        public CommandsModule(InteractionHandler handler)
        {
            _handler = handler;
        }

        [SlashCommand("ping", "Pings the bot and returns its latency.")]
        public async Task GreetUserAsync()
            => await RespondAsync(text: $":ping_pong: It took me {Context.Client.Latency}ms to respond to you!", ephemeral: true);

        [SlashCommand("colorme", "Add yourself a server color!.")]
        public async Task ColorUser(string hex)
        {
            var color = new System.Drawing.Color();

            try
            {
                if (!hex.StartsWith("#"))
                    hex = string.Format("#{0}", hex);
                color = ColorTranslator.FromHtml(hex);
            }
            catch (Exception)
            {
                await RespondAsync(text: "Invalid color code.", ephemeral: true);
                return;
            }

            try
            {
                var role = await Context.Guild.CreateRoleAsync(Context.User.Username);

                Discord.Color dcolor = new Discord.Color(color.R, color.G, color.B);

                await role.ModifyAsync(x => x.Color = dcolor);

                await Context.Guild.Users.FirstOrDefault(x => x.Id == Context.User.Id).AddRoleAsync(role);

                await RespondAsync(text: "Your pretty color was applied!", ephemeral: true);
            }
            catch (Exception e)
            {
                await RespondAsync(text: $"An error ocurred. Reach an admin for more information.", ephemeral: true);
                Log.Error(e.InnerException, e.Message);
            }
        }
    }
}
