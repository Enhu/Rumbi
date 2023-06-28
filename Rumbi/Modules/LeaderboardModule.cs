using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Rumbi.Data;
using Rumbi.Data.Config;
using Rumbi.Data.Models;
using Serilog;

namespace Rumbi.Modules
{
    [DefaultMemberPermissions(
        GuildPermission.ManageGuild | GuildPermission.KickMembers | GuildPermission.BanMembers
    )]
    public class LeaderboardModule : InteractionModuleBase<SocketInteractionContext>
    {
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

        [DefaultMemberPermissions(
            GuildPermission.ManageGuild | GuildPermission.KickMembers | GuildPermission.BanMembers
        )]
        [Group("announcement", "Create a new leadearboard announcement.")]
        public class AnnouncementGroup : InteractionModuleBase<SocketInteractionContext>
        {
            private readonly RumbiContext _dbContext;
            private readonly AppConfig _config;

            private static readonly Random random = new();

            public AnnouncementGroup(RumbiContext context, AppConfig config)
            {
                _dbContext = context;
                _config = config;
            }

            [SlashCommand("ping", "Pings the runner role.")]
            public async Task CreateLeaderboardAnnouncementWithPing(
                [Summary(
                    name: "attachment",
                    description: "Any type of attachment, from an image to a file. "
                )]
                    IAttachment? file = null
            )
            {
                HandleAttachment(file);

                await RespondWithModalAsync<AnnouncementModal>($"ann_modal,{true}");
            }

            [SlashCommand("no-ping", "For announcements that don't need pings.")]
            public async Task CreateLeaderboardAnnouncementWithoutPing(
                [Summary(
                    name: "attachment",
                    description: "Any type of attachment, from an image to a file. "
                )]
                    IAttachment? file = null
            )
            {
                HandleAttachment(file);

                await RespondWithModalAsync<AnnouncementModal>($"ann_modal,{false}");
            }

            [ModalInteraction("ann_modal,*", ignoreGroupNames: true)]
            public async Task HandleNewAnnouncement(bool ping, AnnouncementModal modal)
            {
                await DeferAsync();

                var announcementChannel =
                    Context.Guild.Channels.FirstOrDefault(
                        x => x.Id == _config.ChannelConfig.LbAnnouncements
                    ) as SocketTextChannel;
                var runnerRole = Context.Guild.Roles.FirstOrDefault(
                    x => x.Id == _config.RoleConfig.Runner
                );

                if (announcementChannel == null)
                {
                    await FollowupAsync("The announcement channel doesn't exist.", ephemeral: true);
                    return;
                }
                if (ping && runnerRole == null)
                {
                    await FollowupAsync("The runner role doesn't exist.", ephemeral: true);
                    return;
                }

                try
                {
                    Announcement announcement = BuildAnnouncement(modal);

                    if (ping)
                    {
                        await SendAnnouncementMessagePing(
                            modal,
                            announcementChannel,
                            runnerRole,
                            announcement
                        );
                    }
                    else
                    {
                        await SendAnnouncementMessageNoPing(
                            modal,
                            announcementChannel,
                            announcement
                        );
                    }

                    await FollowupAsync("Announcement sent!", ephemeral: true);
                }
                catch (Exception e)
                {
                    Log.Error("An error ocurred trying to send an announcement.");
                    Log.Error(e, e.Message, e.InnerException);
                    await FollowupAsync(
                        "An error ocurred. Please contact an admin for help.",
                        ephemeral: true
                    );
                }
            }

            private async Task SendAnnouncementMessageNoPing(
                AnnouncementModal modal,
                SocketTextChannel? announcementChannel,
                Announcement announcement
            )
            {
                var title = string.IsNullOrEmpty(modal.AnnouncementTitle)
                    ? string.Empty
                    : $"**{modal.AnnouncementTitle}**\n\n";

                var announcementMessage = await announcementChannel.SendMessageAsync(
                    $"{title}{modal.AnnouncementContent}\n\n**Hat moderation team.**"
                );

                if (announcement.Attachment != null)
                    await SendAttachmentMessage(announcementChannel, announcement);

                announcement.MessageId = announcementMessage.Id;

                _dbContext.Announcements.Add(announcement);
                _dbContext.SaveChanges();
            }

            private async Task SendAnnouncementMessagePing(
                AnnouncementModal modal,
                SocketTextChannel? announcementChannel,
                SocketRole? runnerRole,
                Announcement announcement
            )
            {
                await runnerRole.ModifyAsync(x => x.Mentionable = true);

                var title = string.IsNullOrEmpty(modal.AnnouncementTitle)
                    ? string.Empty
                    : $"**{modal.AnnouncementTitle}**\n\n";
                var announcementMessage = await announcementChannel.SendMessageAsync(
                    $"{title}{modal.AnnouncementContent} \n\n**Hat moderation team.** {runnerRole.Mention}"
                );

                if (announcement.Attachment != null)
                    await SendAttachmentMessage(announcementChannel, announcement);

                await runnerRole.ModifyAsync(x => x.Mentionable = false);

                announcement.MessageId = announcementMessage.Id;

                _dbContext.Announcements.Add(announcement);
                _dbContext.SaveChanges();
            }

            private static async Task SendAttachmentMessage(
                SocketTextChannel? announcementChannel,
                Announcement announcement
            )
            {
                var attachmentUrl = string.Format(
                    "https://cdn.discordapp.com/ephemeral-attachments/{0}/{1}/{2}",
                    announcement.Attachment.MediumId,
                    announcement.Attachment.FileId,
                    announcement.Attachment.FileName
                );
                var attachmentMessageId = await announcementChannel.SendMessageAsync(attachmentUrl);

                announcement.Attachment.MessageId = attachmentMessageId.Id;
            }

            private Announcement BuildAnnouncement(AnnouncementModal modal)
            {
                var announcementId = RandomString();

                while (_dbContext.Announcements.Any(x => x.Id == announcementId)) // Makes ID unique
                    announcementId = RandomString();

                var announcement = new Announcement
                {
                    Id = announcementId,
                    Content = modal.AnnouncementContent,
                    Title = modal.AnnouncementTitle,
                };

                var tempFile = _dbContext.TemporalFiles.FirstOrDefault();

                if (tempFile != null)
                {
                    var annAttachment = new AnnouncementAttachment
                    {
                        FileId = tempFile.FileId,
                        FileName = tempFile.FileName,
                        MediumId = tempFile.MediumId
                    };

                    announcement.Attachment = annAttachment;
                }

                return announcement;
            }

            private void HandleAttachment(IAttachment? attachment)
            {
                _dbContext.TemporalFiles.RemoveRange(_dbContext.TemporalFiles);
                _dbContext.SaveChanges();

                if (attachment == null)
                    return;

                var tempFile = new TemporalFile
                {
                    FileId = attachment.Id,
                    FileName = attachment.Filename,
                    MediumId = ulong.Parse(attachment?.Url.Split("/")[4]), // Gets the medium the attachment gets sent through.
                };

                _dbContext.TemporalFiles.Add(tempFile);
                _dbContext.SaveChanges();
            }

            private static string RandomString()
            {
                const string pool = "abcdefghijklmnopqrstuvwxyz0123456789";
                var chars = Enumerable.Range(0, 5).Select(x => pool[random.Next(0, pool.Length)]);
                return new string(chars.ToArray());
            }
        }

        public class AnnouncementModal : IModal
        {
            public string Title => "New leaderboard announcement";

            [InputLabel("Announcement Title")]
            [RequiredInput(false)]
            [ModalTextInput(
                "announcement_title",
                TextInputStyle.Short,
                placeholder: "A simple title for the announcement (optional).",
                maxLength: 50
            )]
            public string AnnouncementTitle { get; set; } = string.Empty;

            [InputLabel("Content")]
            [ModalTextInput(
                "announcement_content",
                TextInputStyle.Paragraph,
                placeholder: "The content of the announcement.",
                maxLength: 3900
            )]
            public string AnnouncementContent { get; set; }
        }
    }
}
