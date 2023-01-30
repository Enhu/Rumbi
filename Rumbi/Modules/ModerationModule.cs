using Discord;
using Discord.Interactions;
using Rumbi.Data;
using Rumbi.Data.Models;
using Serilog;

namespace Rumbi.Modules
{
    [DefaultMemberPermissions(GuildPermission.ManageGuild & GuildPermission.BanMembers)]
    public class ModerationModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly RumbiContext _dbContext;

        public ModerationModule(RumbiContext context)
        {
            _dbContext = context;
        }

        [RequireOwner]
        [SlashCommand("save-color-roles", "Finds all the unsaved color roles and tries to save them on the dabatase.")]
        public async Task SaveColorRoles()
        {
            await DeferAsync();

            var users = Context.Guild.Users.Where(x => x.Roles.Count() > 1);
            var savedCount = 0;

            try
            {
                foreach (var user in users)
                {
                    var colorRole = user.Roles
                        .Where(x => x.Name == user.Username)
                        .Where(x => !x.IsManaged)
                        .Where(x => !x.IsHoisted)
                        .Where(x => !x.Permissions.KickMembers || !x.Permissions.BanMembers || 
                        !x.Permissions.ManageGuild || !x.Permissions.Administrator)
                        .FirstOrDefault();

                    if (colorRole == null) continue;
                    if (_dbContext.GuildUsers.Any(x => x.Id == user.Id)) continue;

                    _dbContext.GuildUsers.Add(new User { Id = user.Id, ColorRoleId = colorRole.Id, Color = colorRole.Color.RawValue, Username = user.Username });
                    _dbContext.SaveChanges();

                    savedCount++;
                }

                if (savedCount == 0) { await FollowupAsync(text: "No role colors to save."); return; }

                await FollowupAsync(text: $"Successfully saved {savedCount} color roles.");
            }
            catch (Exception e)
            {
                Log.Error("An error ocurred trying to save roles on the database.");
                Log.Error(e, e.Message, e.InnerException);
            }
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
                    .Where(x => !x.IsHoisted)
                    .Where(x => !x.Permissions.KickMembers || !x.Permissions.BanMembers || 
                    !x.Permissions.ManageGuild || !x.Permissions.Administrator)
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
            public async Task DeleteUnusedRoles([Summary(name: "exclude", description: "The role IDs to excluse in the deletion, separated by commas.")] string? excludes = null)
            {
                await DeferAsync();

                var currentRole = string.Empty;
                var currentRoleID = string.Empty;

                try
                {
                    var rolesToExclude = excludes != null ? excludes.Replace(" ", "").Split(',').Select(ulong.Parse).ToList() 
                        : new List<ulong>();

                    var unusedRolesList = Context.Guild.Roles
                        .Where(x => x.Members.Count() == 0)
                        .Where(x => !x.IsManaged)
                        .Where(x => !x.IsHoisted)
                        .Where(x => !x.Permissions.KickMembers || !x.Permissions.BanMembers ||
                        !x.Permissions.ManageGuild || !x.Permissions.Administrator)
                        .Where(x => !rolesToExclude.Contains(x.Id));

                    if (!unusedRolesList.Any())
                    {
                        await FollowupAsync(text: "No unused roles found.");
                        return;
                    }

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
