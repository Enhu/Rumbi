using Discord.Interactions;
using Rumbi.Data;
using Serilog;
using System.Drawing;

namespace Rumbi.Modules
{
    public class GeneralModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly RumbiContext _dbContext;

        public GeneralModule(RumbiContext context)
        {
            _dbContext = context;
        }

        [SlashCommand("ping", "Pings the bot and returns its latency.")]
        public async Task GreetUserAsync()
            => await RespondAsync(text: $":ping_pong: It took me {Context.Client.Latency}ms to respond to you!", ephemeral: true);

        [SlashCommand("color-me", "Add yourself a server color!.")]
        public async Task ColorUser(string hex)
        {
            var color = new Color();

            try
            {
                if (!hex.StartsWith("#"))
                    hex = string.Format("#{0}", hex);

                color = ColorTranslator.FromHtml(hex);
            }
            catch (Exception)
            {
                await RespondAsync(text: "Invalid color code.", ephemeral: true);
                Log.Warning($"String used: {hex}");
                return;
            }

            Discord.Color dcolor = new Discord.Color(color.R, color.G, color.B);

            try
            {
                await AssingRole(dcolor);

                await RespondAsync(text: "Your pretty color was applied!", ephemeral: true);
            }
            catch (Exception e)
            {
                await RespondAsync(text: $"An error ocurred. Reach an admin for more information.", ephemeral: true);
                Log.Error(e.InnerException, e.Message, e.InnerException);
            }
        }

        private async Task AssingRole(Discord.Color dcolor)
        {
            var role = _dbContext.Roles.FirstOrDefault(x => x.UserId == Context.User.Id);

            if (role == null)
            {
                var drole = await Context.Guild.CreateRoleAsync(Context.User.Username);

                await drole.ModifyAsync(x => x.Color = dcolor);

                await Context.Guild.Users.FirstOrDefault(x => x.Id == Context.User.Id).AddRoleAsync(drole);
            }
            else
            {
                var drole = Context.Guild.GetRole(role.Id);

                await drole.ModifyAsync(x => x.Color = dcolor);
            }
        }
    }
}
