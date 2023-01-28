using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Rumbi.Data;

namespace Rumbi.Modules
{
    [DefaultMemberPermissions(GuildPermission.ManageGuild & GuildPermission.KickMembers & GuildPermission.BanMembers)]
    public class LeaderboardModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly RumbiContext _dbContext;

        public LeaderboardModule(RumbiContext context)
        {
            _dbContext = context;
        }


        [SlashCommand("poll", "Create a new leadearboard poll")]
        public async Task CreateLeaderboardPoll()
        {
            throw new NotImplementedException();
        }

        [SlashCommand("close-poll", "Create a new leadearboard announcement")]
        public async Task CloseLeaderboardPoll()
        {
            throw new NotImplementedException();

        }
        [Group("announcement", "Create a new leadearboard announcement.")]
        public class AnnouncementGroup : InteractionModuleBase<SocketInteractionContext>
        {
            [SlashCommand("ping", "Pings the runner role.")]
            public async Task CreateLeaderboardAnnouncementWithPing()
            {
                await RespondWithModalAsync<AnnouncementModal>($"announcement_modal, {true}");
            }

            [SlashCommand("no-ping", "For announcements that don't need pings.")]
            public async Task CreateLeaderboardAnnouncementWithoutPing()
            {
                await RespondWithModalAsync<AnnouncementModal>($"announcement_modal, {false}");
            }

            [ModalInteraction("announcement_modal,*", ignoreGroupNames: true)]
            public async Task HandleNewAnnouncement(bool ping, AnnouncementModal modal)
            {
                await DeferAsync();

                var announcementChannel = Context.Guild.Channels.FirstOrDefault(x => x.Id == 1068101892594475068) as SocketTextChannel; //config
                var runnerRole = Context.Guild.Roles.FirstOrDefault(x => x.Id == 1067503350104469514);

                if (announcementChannel == null) { await FollowupAsync("The announcement channel doesn't exist."); return; }
                if (ping && runnerRole == null) { await FollowupAsync("The runner role doesn't exist."); return; }

                if (announcementChannel != null)
                {
                    if (ping)
                    {
                        await runnerRole.ModifyAsync(x => x.Mentionable = true);
                        await announcementChannel.SendMessageAsync($"{modal.AnnouncementTitle}\n\n{modal.AnnouncementContent} \n{runnerRole.Mention}");
                        await runnerRole.ModifyAsync(x => x.Mentionable = false);
                    }
                    else
                    {
                        await announcementChannel.SendMessageAsync($"{modal.AnnouncementTitle}\n\n{modal.AnnouncementContent}");
                        await announcementChannel.SendMessageAsync($"{modal.AnnouncementContent}");
                    }

                    await FollowupAsync("Announcement sent!", ephemeral: true);
                    return;
                }

                await FollowupAsync("The announcement channel doesn't exist.");
            }
        }

        public class AnnouncementModal : IModal
        {
            public string Title => "New leaderboard announcement";

            [InputLabel("Announcement Title")]
            [RequiredInput(false)]
            [ModalTextInput("announcement_title", TextInputStyle.Short, placeholder: "A simple title for the announcement.", maxLength: 50)]
            public string AnnouncementTitle { get; set; } = string.Empty; 

            [InputLabel("Content")]
            [ModalTextInput("announcement_content", TextInputStyle.Paragraph, placeholder: "The content of the announcement..", maxLength: 3000)]
            public string AnnouncementContent { get; set; }
        }
    }
}
