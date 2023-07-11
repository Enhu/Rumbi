using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Rumbi.Data;
using Rumbi.Data.Config;
using Rumbi.Data.Models;
using Serilog;

namespace Rumbi.Modules.Admin
{
    [RequireOwner]
    public class DevModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly RumbiContext _dbContext;

        public DevModule(RumbiContext context)
        {
            _dbContext = context;
        }

        [SlashCommand(
            "save-color-roles",
            "Finds all the unsaved color roles and tries to save them on the dabatase."
        )]
        public async Task SaveColorRoles()
        {
            await DeferAsync(ephemeral: true);

            var users = Context.Guild.Users.Where(x => x.Roles.Count > 1);
            var savedCount = 0;

            try
            {
                foreach (var user in users)
                {
                    var colorRole = user.Roles
                        .Where(
                            x => x.Name.Equals(user.Username, StringComparison.OrdinalIgnoreCase)
                        )
                        .Where(x => !x.IsManaged)
                        .Where(x => !x.IsHoisted)
                        .Where(
                            x =>
                                !x.Permissions.KickMembers
                                || !x.Permissions.BanMembers
                                || !x.Permissions.ManageGuild
                                || !x.Permissions.Administrator
                        )
                        .FirstOrDefault();

                    if (colorRole == null)
                        continue;

                    if (_dbContext.Users.Any(x => x.Id == user.Id))
                        continue;

                    _dbContext.Users.Add(
                        new User
                        {
                            Id = user.Id,
                            ColorRoleId = colorRole.Id,
                            Color = colorRole.Color.RawValue,
                            Username = user.Username
                        }
                    );
                    _dbContext.SaveChanges();

                    savedCount++;
                }

                if (savedCount == 0)
                {
                    await FollowupAsync(text: "No role colors to save.", ephemeral: true);
                    return;
                }

                await FollowupAsync(text: $"Successfully saved {savedCount} color roles.", ephemeral: true);
            }
            catch (Exception e)
            {
                await FollowupAsync(text: "An error ocurred. Check the logs for more information.", ephemeral: true);
                Log.Error("An error ocurred trying to save roles on the database.");
                Log.Error(e, e.Message, e.InnerException);
            }
        }

        [RequireOwner]
        [Group("streaming", "Group for the streaming role.")]
        public class StreamingRoleGroup : InteractionModuleBase<SocketInteractionContext>
        {
            private readonly AppConfig _config;

            public StreamingRoleGroup(AppConfig config)
            {
                _config = config;
            }

            [SlashCommand("clear-all", "Clears all the streaming roles.")]
            public async Task ClearAllStreamingRoles()
            {
                var streamingRole = Context.Guild.GetRole(_config.RoleConfig.Streaming);
                var streamingUsers = Context.Guild.Users
                    .Where(x => x.Roles.Any(x => x.Id == _config.RoleConfig.Streaming))
                    .ToList();

                if (streamingRole == null)
                {
                    await RespondAsync(text: "Couldn't find the streaming role", ephemeral: true);
                    return;
                }
                if (streamingUsers.Count < 1)
                {
                    await RespondAsync(text: "No users using the role.", ephemeral: true);
                    return;
                }

                foreach (var user in streamingUsers)
                {
                    await user.RemoveRoleAsync(streamingRole);
                }

                await RespondAsync(text: "Cleared streaming roles", ephemeral: true);
            }

            [SlashCommand("clear", "Give a user ID to clear the role.")]
            public async Task ClearStreamingRole(
                [Summary(name: "ID", description: "The user ID.")] string ulongId
            )
            {
                if (!ulong.TryParse(ulongId, out var userId))
                {
                    await RespondAsync(text: "Invalid ID", ephemeral: true);
                    return;
                }

                var streamingUser = Context.Guild.Users
                    .Where(
                        x =>
                            x.Roles.Any(x => x.Id == _config.RoleConfig.Streaming) && x.Id == userId
                    )
                    .FirstOrDefault();

                if (streamingUser == null)
                {
                    await RespondAsync(text: "User doesn't have the role.");
                    return;
                }

                await streamingUser.RemoveRoleAsync(_config.RoleConfig.Streaming);

                await RespondAsync(text: "Cleared streaming role", ephemeral: true);
            }
        }

        [RequireOwner]
        [Group("unused-roles", "Group for unused roles")]
        public class UnusedRolesGroup : InteractionModuleBase<SocketInteractionContext>
        {
            [SlashCommand("list-all", "Finds and lists all the unused roles.")]
            public async Task ListUnusedRoles()
            {
                await DeferAsync(ephemeral: true);

                var unusedRolesList = string.Join(
                    ",\n",
                    Context.Guild.Roles
                        .Where(x => !x.Members.Any())
                        .Where(x => !x.IsManaged)
                        .Where(x => !x.IsHoisted)
                        .Where(
                            x =>
                                !x.Permissions.KickMembers
                                || !x.Permissions.BanMembers
                                || !x.Permissions.ManageGuild
                                || !x.Permissions.Administrator
                        )
                        .Select(x => $"{x.Mention} ID:{x.Id}")
                );

                if (!unusedRolesList.Any())
                {
                    await FollowupAsync(text: "No unused roles found.", ephemeral: true);
                    return;
                }

                var embed = new EmbedBuilder()
                    .WithTitle("Unused roles")
                    .WithDescription(unusedRolesList)
                    .WithColor(Color.Green)
                    .WithCurrentTimestamp()
                    .Build();

                await FollowupAsync(embed: embed, ephemeral: true);
            }

            [SlashCommand("delete-all", "Finds all the unused roles and deletes them.")]
            public async Task DeleteUnusedRoles(
                [Summary(
                    name: "exclude",
                    description: "The role IDs to excluse in the deletion, separated by commas."
                )]
                    string? excludes = null
            )
            {
                await DeferAsync(ephemeral: true);

                var currentRole = string.Empty;
                var currentRoleID = string.Empty;

                try
                {
                    var rolesToExclude =
                        excludes?.Replace(" ", string.Empty).Split(',').Select(ulong.Parse).ToList()
                        ?? new List<ulong>();

                    var unusedRolesList = Context.Guild.Roles
                        .Where(x => !x.Members.Any())
                        .Where(x => !x.IsManaged)
                        .Where(x => !x.IsHoisted)
                        .Where(
                            x =>
                                !x.Permissions.KickMembers
                                || !x.Permissions.BanMembers
                                || !x.Permissions.ManageGuild
                                || !x.Permissions.Administrator
                        )
                        .Where(x => !rolesToExclude.Contains(x.Id));

                    if (!unusedRolesList.Any())
                    {
                        await FollowupAsync(text: "No unused roles found.", ephemeral: true);
                        return;
                    }

                    foreach (var role in unusedRolesList)
                    {
                        currentRole = role.Name;
                        currentRoleID = role.Id.ToString();
                        await role.DeleteAsync();
                    }

                    await FollowupAsync(text: "Successfully deleted all unused roles.", ephemeral: true);
                }
                catch (Exception e)
                {
                    await FollowupAsync(
                        text: "An error ocurred. Check the logs for more information."
                    , ephemeral: true);
                    Log.Error(
                        $"An error ocurred trying to delete role: name:  {currentRole}, ID: {currentRoleID} "
                    );
                    Log.Error(e, e.Message, e.InnerException);
                }
            }
        }
    }
}