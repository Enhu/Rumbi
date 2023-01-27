using Discord;
using Discord.Interactions;
using Rumbi.Data;
using Rumbi.Data.Models;
using Serilog;

namespace Rumbi.Modules
{
    [DefaultMemberPermissions(GuildPermission.ManageGuild | GuildPermission.BanMembers)]
    public class ModerationModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly RumbiContext _dbContext;

        public ModerationModule(RumbiContext context)
        {
            _dbContext = context;
        }

        [RequireOwner]
        [SlashCommand("save-color-roles", "Finds all the unsaved color roles and saves them on the dabatase.")]
        public async Task ClearColorRoles()
        {
            await DeferAsync();

            var users = Context.Guild.Users.Where(x => x.Roles.Count() > 1);

            foreach (var user in users)
            {
                var colorRole = user.Roles.FirstOrDefault(x => x.Name == user.Username);

                if (colorRole != null && _dbContext.Roles.Any(x => x.Id == x.UserId))
                    continue;

                _dbContext.Roles.Add(new Role { Id = colorRole.Id, UserId = user.Id, Color = colorRole.Color.RawValue });
                _dbContext.SaveChanges();
            }

            await FollowupAsync(text: "Successfully saved all color roles", ephemeral: true);
        }

        [RequireOwner]
        [Group("unused-roles", "Group for unused roles")]
        public class UnusedRolesGroup : InteractionModuleBase<SocketInteractionContext>
        {
            [SlashCommand("list", "Finds and lists all the unused roles.")]
            public async Task ListUnusedRoles()
            {
                await DeferAsync();

                var unusedRolesList = string.Join(",\n", Context.Guild.Roles
                    .Where(x => x.Members.Count() == 0)
                    .Where(x => !x.IsManaged)
                    .Where(x => !x.Permissions.ManageGuild || !x.Permissions.KickMembers)
                    .Select(x => x.Mention));

                if (!unusedRolesList.Any())
                {
                    await FollowupAsync(text: "No unused roles found.");
                    return;
                }

                var embed = new EmbedBuilder()
                    .WithTitle("Unused roles")
                    .WithDescription(unusedRolesList)
                    .WithColor(Color.Green)
                    .WithCurrentTimestamp()
                    .Build();

                await FollowupAsync(embed: embed);
            }

            [SlashCommand("delete", "Finds all the unused roles and deletes them.")]
            public async Task DeleteUnusedRoles()
            {
                await DeferAsync();

                var unusedRolesList = Context.Guild.Roles
                    .Where(x => x.Members.Count() == 0)
                    .Where(x => !x.IsManaged)
                    .Where(x => !x.Permissions.ManageGuild || !x.Permissions.KickMembers);

                if (!unusedRolesList.Any())
                {
                    await FollowupAsync(text: "No unused roles found.");
                    return;
                }

                var currentRole = string.Empty;
                var currentRoleID = string.Empty;

                try
                {

                    foreach (var role in unusedRolesList)
                    {
                        currentRole = role.Name;
                        currentRoleID = role.Id.ToString();
                        await role.DeleteAsync();
                    }

                }
                catch (Exception e)
                {
                    await FollowupAsync(text: "An error ocurred. Check the logs for more information.");
                    Log.Error($"An error ocurred trying to delete role: name:  {currentRole}, ID: {currentRoleID} ");
                    Log.Error(e, e.Message, e.InnerException);
                    return;
                }

                await FollowupAsync(text: "Successfully deleted all unused roles.");
            }
        }
    }
}
