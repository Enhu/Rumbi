using Discord;
using Discord.Interactions;
using Rumbi.Data;
using Rumbi.Data.Models;
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

        [SlashCommand("about", "Information about this bot.")]
        public async Task About()
        {

        }

        [SlashCommand("color-me", "Add yourself a server color!.")]
        public async Task ColorUser(string hex)
        {
            await DeferAsync();

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
                Log.Warning($"String used: {hex}");
                return;
            }

            Discord.Color dcolor = new Discord.Color(color.R, color.G, color.B);

            try
            {
                await AssingRole(dcolor);

                await FollowupAsync(text: "Your pretty color was applied!", ephemeral: true);
            }
            catch (Exception e)
            {
                await FollowupAsync(text: $"An error ocurred. Reach an admin for more information.", ephemeral: true);
                Log.Error(e.InnerException, e.Message, e.InnerException);
            }
        }

        private async Task AssingRole(Discord.Color dcolor)
        {
            var user = _dbContext.GuildUsers.FirstOrDefault(x => x.Id == Context.User.Id);

            if (user == null)
            {
                var drole = await Context.Guild.CreateRoleAsync(Context.User.Username);

                await drole.ModifyAsync(x => x.Color = dcolor);

                await Context.Guild.Users.FirstOrDefault(x => x.Id == Context.User.Id).AddRoleAsync(drole);

                user = new User
                {
                    Id = Context.User.Id,
                    Color = dcolor.RawValue,
                    ColorRoleId = drole.Id,
                    Username = Context.User.Username,
                };

                _dbContext.GuildUsers.Add(user);
                _dbContext.SaveChanges();
            }
            else
            {
                var drole = Context.Guild.GetRole(user.ColorRoleId);

                await drole.ModifyAsync(x => x.Color = dcolor);

                user.Color = dcolor.RawValue;

                _dbContext.Update(user);
                _dbContext.SaveChanges();
            }
        }
    }
}
