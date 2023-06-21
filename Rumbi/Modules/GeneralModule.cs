using Discord;
using Discord.Interactions;
using Rumbi.Data;
using Rumbi.Data.Config;
using Rumbi.Data.Models;
using Serilog;
using System.Drawing;
using System.Security.Cryptography.X509Certificates;

namespace Rumbi.Modules
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

        [SlashCommand("color-me", "Add yourself a server color!.")]
        public async Task ColorUser(string hex)
        {
            await DeferAsync();

            try
            {
                if (!hex.StartsWith("#"))
                    hex = string.Format("#{0}", hex);

                var color = ColorTranslator.FromHtml(hex);

                Discord.Color dColor = new(color.R, color.G, color.B);

                try
                {
                    await AssingRole(dColor);

                    await FollowupAsync(text: "Your pretty color was applied!", ephemeral: true);
                }
                catch (Exception e)
                {
                    await FollowupAsync(
                        text: $"An error ocurred. Reach an admin for more information.",
                        ephemeral: true
                    );
                    Log.Error(e.InnerException, e.Message, e.InnerException);
                }
            }
            catch (Exception)
            {
                await RespondAsync(text: "Invalid color code.", ephemeral: true);
                Log.Warning($"String used: {hex}");
            }
        }

        private async Task AssingRole(Discord.Color dColor)
        {
            var dbUser = _dbContext.GuildUsers.FirstOrDefault(x => x.Id == Context.User.Id);

            if (dbUser == null)
            {
                var guildRole = await Context.Guild.CreateRoleAsync(Context.User.Username);

                var verifierRole = Context.Guild.GetRole(_config.RoleConfig.Verifier);
                int position = verifierRole.Position - 1;

                await guildRole.ModifyAsync(x =>
                {
                    x.Color = dColor;
                    x.Position = position;
                });

                await Context.Guild.Users
                    .FirstOrDefault(x => x.Id == Context.User.Id)
                    .AddRoleAsync(guildRole);

                dbUser = new User
                {
                    Id = Context.User.Id,
                    Color = dColor.RawValue,
                    ColorRoleId = guildRole.Id,
                    Username = Context.User.Username,
                };

                _dbContext.GuildUsers.Add(dbUser);
                _dbContext.SaveChanges();
            }
            else
            {
                var guildRole = Context.Guild.GetRole(dbUser.ColorRoleId);

                var guildUser =
                    Context.Guild.Users.FirstOrDefault(x => x.Id == Context.User.Id)
                    ?? throw new Exception("User not found inside the guild");

                if (guildUser.Roles.Contains(guildRole) != true)
                {
                    await guildRole.ModifyAsync(x => x.Color = dColor);
                    await guildUser.AddRoleAsync(guildRole);
                    dbUser.Color = dColor.RawValue;
                }
                else
                {
                    await guildRole.ModifyAsync(x => x.Color = dColor);
                    dbUser.Color = dColor.RawValue;
                }

                _dbContext.Update(dbUser);
                _dbContext.SaveChanges();
            }
        }
    }
}
