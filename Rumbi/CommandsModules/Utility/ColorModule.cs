using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Rumbi.Data;
using Rumbi.Data.Config;
using Rumbi.Data.Models;
using Serilog;

namespace Rumbi.CommandsModules.Utility
{
    [Group("color", "Color utilities")]
    public class ColorModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly RumbiContext _dbContext;
        private readonly AppConfig _config;

        public ColorModule(RumbiContext context, AppConfig config)
        {
            _dbContext = context;
            _config = config;
        }

        [SlashCommand("me", "Give yourself a color")]
        public async Task ColorUser([Summary("Color", "Example: #ffffff or ffffff")] string hex)
        {
            await DeferAsync();

            try
            {
                if (!hex.StartsWith("#"))
                    hex = $"#{hex}";

                var hexColor = ColorTranslator.FromHtml(hex);

                Discord.Color roleColor = new(hexColor.R, hexColor.G, hexColor.B);

                try
                {
                    await AssingRole(roleColor);
                    await FollowupAsync(text: "Color applied!", ephemeral: true);
                }
                catch (Exception e)
                {
                    await FollowupAsync(text: e.Message, ephemeral: true);
                    Log.Error(e.InnerException, e.Message, e.InnerException);
                }
            }
            catch (Exception)
            {
                await RespondAsync(text: "Invalid color code.", ephemeral: true);
                Log.Warning($"String used: {hex}");
            }
        }

        [SlashCommand("remove", "Remove your color role")]
        public async Task RemoveColor()
        {
            var socketUser = Context.User as SocketGuildUser;

            var roleId = _dbContext.Users
                .Where(x => x.Id == socketUser.Id)
                .Select(y => y.ColorRoleId)
                .SingleOrDefault();

            if (roleId == 0)
            {
                await RespondAsync(text: "Color not found", ephemeral: true);
                return;
            }

            await socketUser.RemoveRoleAsync(roleId);

            await RespondAsync(text: "Color removed", ephemeral: true);
        }

        private async Task AssingRole(Discord.Color dColor)
        {
            var dbUser = _dbContext.Users.FirstOrDefault(x => x.Id == Context.User.Id);

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

                _dbContext.Users.Add(dbUser);
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
