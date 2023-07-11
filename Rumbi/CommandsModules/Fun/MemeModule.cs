using Discord;
using Discord.Interactions;
using Rumbi.Data;
using Rumbi.Data.Models;
using Rumbi.PreConditions;
using Serilog;

namespace Rumbi.CommandsModules.Fun
{
    public class MemeModule : InteractionModuleBase<SocketInteractionContext>
    {
        [Group("meme", "Manage memes")]
        public class MemeGroup : InteractionModuleBase<SocketInteractionContext>
        {
            private int MemeListPageSize { get; set; } = 5;
            private readonly RumbiContext _dbContext;

            public MemeGroup(RumbiContext context)
            {
                _dbContext = context;
            }

            [RequireModerator]
            [SlashCommand("add", "Add a new meme.")]
            public async Task AddMeme(
                [Summary(description: "The trigger for the meme.")] string trigger,
                [Summary(description: "The content of the meme.")] string content
            )
            {
                try
                {
                    var memeExists = _dbContext.Memes.Any(x => x.Trigger == trigger);

                    if (memeExists)
                    {
                        await RespondAsync(
                            "The meme already exists, modify it or use another trigger word.",
                            ephemeral: true
                        );
                        return;
                    }

                    var meme = new Meme { Content = content, Trigger = trigger, };

                    _dbContext.Add(meme);
                    _dbContext.SaveChanges();

                    await RespondAsync("Meme added.", ephemeral: true);
                }
                catch (Exception e)
                {
                    Log.Error(e, e.Message, e.InnerException);
                }
            }

            [RequireModerator]
            [SlashCommand("remove", "Remove a meme.")]
            public async Task AddMeme(
                [Summary(description: "The trigger for the meme.")] string trigger
            )
            {
                try
                {
                    var meme = _dbContext.Memes.FirstOrDefault(x => x.Trigger == trigger);

                    if (meme == null)
                    {
                        await RespondAsync("The meme was not found", ephemeral: true);
                        return;
                    }

                    _dbContext.Memes.Remove(meme);
                    _dbContext.SaveChanges();

                    await RespondAsync("Meme removed.", ephemeral: true);
                }
                catch (Exception e)
                {
                    Log.Error(e, e.Message, e.InnerException);
                }
            }

            [RequireModerator]
            [SlashCommand("modify", "Modify a meme.")]
            public async Task ModifyMeme(
                [Summary(description: "The trigger for the meme.")] string trigger,
                [Summary(description: "The content of the meme.")] string content
            )
            {
                try
                {
                    var meme = _dbContext.Memes.FirstOrDefault(x => x.Trigger == trigger);

                    if (meme == null)
                    {
                        await RespondAsync("The meme doesn't exist.", ephemeral: true);
                        return;
                    }

                    meme.Content = content;

                    _dbContext.Update(meme);
                    _dbContext.SaveChanges();

                    await RespondAsync("Meme modified.", ephemeral: true);
                }
                catch (Exception e)
                {
                    Log.Error(e, e.Message, e.InnerException);
                }
            }

            [SlashCommand("list-all", "Gets a list of all memes.")]
            public async Task ListAllMemes()
            {
                await DeferAsync();

                var pageNumber = 0;
                var userId = Context.Interaction.User.Id;

                if (!_dbContext.Memes.Any())
                {
                    await FollowupAsync(text: "No memes found... tfw memeless :sob:");
                    return;
                }

                await FollowupAsync(
                    embed: GetMemeList(pageNumber),
                    components: GetMemeListPageButtons(pageNumber.ToString(), userId.ToString())
                );
            }

            [ComponentInteraction("memePage_*,*,*", ignoreGroupNames: true)]
            public async Task ChangeMemePageAsync(string page, string pageNumber, string userId)
            {
                await DeferAsync();

                if (Context.Interaction.User.Id != ulong.Parse(userId))
                    return;

                if (page.Equals("r"))
                {
                    var currentPage = (int.Parse(pageNumber) + 1);

                    var embed = GetMemeList(currentPage);

                    if (embed == null)
                        return;

                    await ModifyOriginalResponseAsync(x =>
                    {
                        x.Embed = embed;
                        x.Components = GetMemeListPageButtons(currentPage.ToString(), userId);
                    });
                }
                else
                {
                    if (pageNumber.Equals("0"))
                        return;

                    var currentPage = (int.Parse(pageNumber) - 1);

                    await ModifyOriginalResponseAsync(x =>
                    {
                        x.Embed = GetMemeList(currentPage);
                        x.Components = GetMemeListPageButtons(currentPage.ToString(), userId);
                    });
                }
            }

            [ComponentInteraction("delMemeList_*", ignoreGroupNames: true)]
            public async Task DeleteMemeList(string userId)
            {
                await DeferAsync();

                if (Context.Interaction.User.Id != ulong.Parse(userId))
                    return;

                await Context.Interaction.DeleteOriginalResponseAsync();
            }

            private static MessageComponent GetMemeListPageButtons(string pageNumber, string userId)
            {
                var rightButton = new ButtonBuilder()
                    .WithCustomId($"memePage_r,{pageNumber},{userId}")
                    .WithStyle(ButtonStyle.Primary)
                    .WithLabel(">");

                var leftButton = new ButtonBuilder()
                    .WithCustomId($"memePage_l,{pageNumber},{userId}")
                    .WithLabel("<")
                    .WithStyle(ButtonStyle.Primary);

                var deleteButton = new ButtonBuilder()
                    .WithCustomId($"delMemeList_{userId}")
                    .WithLabel("X")
                    .WithStyle(ButtonStyle.Danger);

                var buttonsComponent = new ComponentBuilder()
                    .WithButton(leftButton)
                    .WithButton(rightButton)
                    .WithButton(deleteButton);

                return buttonsComponent.Build();
            }

            private Embed? GetMemeList(int pageNumber)
            {
                if (pageNumber > 0)
                {
                    var embed = new EmbedBuilder()
                        .WithTitle("Memes available")
                        .WithColor(Color.Green)
                        .WithCurrentTimestamp();

                    var memeList = _dbContext.Memes
                        .Skip(pageNumber * MemeListPageSize)
                        .Take(MemeListPageSize)
                        .ToList();

                    if (memeList.Count == 0)
                        return null;

                    var fields = memeList
                        .Select(
                            meme =>
                                new EmbedFieldBuilder
                                {
                                    Name = meme.Trigger,
                                    Value =
                                        meme.Content.Length > 53
                                            ? meme.Content.Substring(0, 50) + "..."
                                            : meme.Content
                                }
                        )
                        .ToList();
                    embed.WithFields(fields);

                    return embed.Build();
                }
                else
                {
                    var embed = new EmbedBuilder()
                        .WithTitle("Memes available")
                        .WithColor(Color.Green)
                        .WithCurrentTimestamp();

                    var memeList = _dbContext.Memes.Take(MemeListPageSize).ToList();
                    var fields = memeList
                        .Select(
                            meme =>
                                new EmbedFieldBuilder
                                {
                                    Name = meme.Trigger,
                                    Value =
                                        meme.Content.Length > 53
                                            ? meme.Content.Substring(0, 50) + "..."
                                            : meme.Content
                                }
                        )
                        .ToList();
                    embed.WithFields(fields);

                    return embed.Build();
                }
            }
        }
    }
}