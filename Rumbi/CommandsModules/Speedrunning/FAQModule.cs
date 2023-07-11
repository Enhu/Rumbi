using Discord;
using Discord.Interactions;
using Rumbi.Data;
using Rumbi.Data.Models;
using Rumbi.Exceptions;
using Rumbi.Modals;
using Rumbi.PreConditions;
using Rumbi.Utils;
using Serilog;

namespace Rumbi.CommandsModules.Speedrunning
{
    internal enum QuestionType
    {
        Strat,
        General
    }

    [Group("faq", "Frequently asked questions about hat speedrunning")]
    public class FAQModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly RumbiContext _dbContext;

        public FAQModule(RumbiContext dbContext)
        {
            _dbContext = dbContext;
        }

        [SlashCommand("general", "General questions about the game!")]
        public async Task ShowGeneralFAQ()
        {
            var type = QuestionType.General.ToString();

            if (!_dbContext.FAQs.Any(x => x.Type.Equals(type)))
            {
                await RespondAsync(BotMessage.NoQuestions, ephemeral: true);
                return;
            }

            await DeferAsync(ephemeral: true);

            await FollowupAsync(
                text: BotMessage.StratsWelcomeMessage,
                components: GetStratsMenuOptions(type),
                ephemeral: true
            );
        }

        [SlashCommand("strats", "Learn about strats!")]
        public async Task ShowStratsFAQ()
        {
            var type = QuestionType.Strat.ToString();

            if (!_dbContext.FAQs.Any(x => x.Type.Equals(type)))
            {
                await RespondAsync(BotMessage.NoQuestions, ephemeral: true);
                return;
            }

            await DeferAsync(ephemeral: true);

            await FollowupAsync(
                text: BotMessage.StratsWelcomeMessage,
                components: GetStratsMenuOptions(type),
                ephemeral: true
            );
        }

        [ComponentInteraction("faqstrats_*", ignoreGroupNames: true)]
        public async Task StratsFAQSelection(string type, string[] selectedQuestion)
        {
            var interactionContext = Context.Interaction as IComponentInteraction;

            var content = _dbContext.FAQs
                .Where(x => x.Type == type && x.Identifier == selectedQuestion[0])
                .Select(y => y.Content)
                .SingleOrDefault();

            await RespondAsync(content, ephemeral: true);
        }

        private MessageComponent GetStratsMenuOptions(string type)
        {
            var selectOptions = _dbContext.FAQs
                .Where(x => x.Type.Equals(type))
                .Select(strat =>
                    new SelectMenuOptionBuilder
                    {
                        Value = strat.Identifier,
                        Description = string.IsNullOrEmpty(strat.Description) ? "-" : strat.Description,
                        Label = strat.Label
                    })
                .ToList();

            var selectMenu = new SelectMenuBuilder()
                .WithCustomId($"faqstrats_{type}")
                .WithPlaceholder(BotMessage.QuestionSelect)
                .WithOptions(selectOptions);

            var messageComponent = new ComponentBuilder().WithSelectMenu(selectMenu).Build();

            return messageComponent;
        }

        [RequireModRole]
        [Group("manage", "Manage questions")]
        public class StratsQuestionsModule : InteractionModuleBase<SocketInteractionContext>
        {
            private readonly RumbiContext _dbContext;

            public StratsQuestionsModule(RumbiContext dbContext)
            {
                _dbContext = dbContext;
            }

            [SlashCommand("strats", "Manage strat questions")]
            public async Task ManageStratsQuestions()
            {
                await DeferAsync(ephemeral: true);

                var type = QuestionType.Strat.ToString();

                var itemsExist = _dbContext.FAQs.Any(x => x.Type.Equals(type));

                var message = itemsExist ? BotMessage.ManagerMessage : BotMessage.ManagerEmpty;
                var components = itemsExist ? GetManagerComponents(type) : GetManagerComponentsNoItems(type);

                await FollowupAsync(text: message,
                    components: components, ephemeral: true);
            }

            [SlashCommand("general", "Manage general questions")]
            public async Task ManageGeneralQuestions()
            {
                await DeferAsync(ephemeral: true);

                var type = QuestionType.General.ToString();

                var itemsExist = _dbContext.FAQs.Any(x => x.Type.Equals(type));

                var message = itemsExist ? BotMessage.ManagerMessage : BotMessage.ManagerEmpty;
                var components = itemsExist ? GetManagerComponents(type) : GetManagerComponentsNoItems(type);

                await FollowupAsync(text: message,
                    components: components, ephemeral: true);
            }

            [ComponentInteraction("manageFAQ_*", ignoreGroupNames: true)]
            public async Task ManageFAQs(string type, string[] selectedQuestion)
            {
                await DeferAsync();

                var identifier = selectedQuestion[0];

                var content = _dbContext.FAQs
                    .Where(x => x.Identifier == identifier)
                    .Select(y => y.Content)
                    .SingleOrDefault();

                await ModifyOriginalResponseAsync(x =>
                {
                    x.Content = BotMessage.ManagerMessage + BotMessage.ManagerQuestionSelected(identifier);
                    x.Components = GetManagerComponents(type: type, disabled: false, identifier = identifier);
                });
            }

            [ComponentInteraction("btnRefresh_*", ignoreGroupNames: true)]
            public async Task ButtonRefresh(string type)
            {
                await DeferAsync();

                var itemsExist = _dbContext.FAQs.Any(x => x.Type.Equals(type));

                var message = itemsExist ? BotMessage.ManagerMessage : BotMessage.ManagerEmpty;
                var components = itemsExist ? GetManagerComponents(type) : GetManagerComponentsNoItems(type);

                await ModifyOriginalResponseAsync(x =>
                {
                    x.Content = message;
                    x.Components = components;
                });
            }

            [ComponentInteraction("btnAdd_*", ignoreGroupNames: true)]
            public async Task ButtonAdd(string type)
            {
                var guid = Guid.NewGuid();
                await Context.Interaction.RespondWithModalAsync<StratQuestionsModal>(
                    $"addFAQMdl_{guid},{type}",
                    null,
                    x => x.WithTitle(BotMessage.QuestionModalAdd)
                );
            }

            [ComponentInteraction("btnEdit_*,*", ignoreGroupNames: true)]
            public async Task ButtonEdit(string identifier, string type)
            {
                var strat = _dbContext.FAQs
                    .Where(x => x.Identifier == identifier)
                    .FirstOrDefault();

                if (strat == null)
                {
                    await FollowupAsync(BotMessage.QuestionNotFound(identifier), ephemeral: true);
                    return;
                }

                var guid = Guid.NewGuid();
                await Context.Interaction.RespondWithModalAsync(
                    $"editFAQMdl_{guid},{strat.Id},{type}",
                    new StratQuestionsModal
                    {
                        Content = strat.Content,
                        Identifier = strat.Identifier,
                        Label = strat.Label,
                        Description = strat.Description,
                    },
                    null,
                    x => x.WithTitle(BotMessage.QuestionModalEdit)
                );
            }

            [ComponentInteraction("btnRemove_*,*", ignoreGroupNames: true)]
            public async Task ButtonRemove(string identifier, string type)
            {
                await DeferAsync();

                var strat = _dbContext.FAQs
                    .Where(x => x.Identifier == identifier && x.Type.Equals(type))
                    .FirstOrDefault();

                if (strat == null)
                {
                    await FollowupAsync(BotMessage.QuestionNotFound(identifier), ephemeral: true);
                    return;
                }

                _dbContext.FAQs.Remove(strat);
                _dbContext.SaveChanges();

                await FollowupAsync(BotMessage.QuestionDeleted, ephemeral: true);
            }

            private MessageComponent GetManagerComponentsNoItems(string type)
            {
                var addButton = new ButtonBuilder()
                    .WithCustomId($"btnAdd_{type}")
                    .WithLabel("Add")
                    .WithStyle(ButtonStyle.Primary);

                var refreshButton = new ButtonBuilder()
                    .WithCustomId($"btnRefresh_{type}")
                    .WithLabel("Refresh")
                    .WithStyle(ButtonStyle.Secondary);

                var crudButtons = new ActionRowBuilder()
                    .WithButton(addButton)
                    .WithButton(refreshButton);

                var messageComponent = new ComponentBuilder()
                    .WithRows(new List<ActionRowBuilder>
                    {
                        crudButtons
                    })
                    .Build();

                return messageComponent;
            }

            private MessageComponent GetManagerComponents(string type, bool disabled = true, string? identifier = null)
            {
                var selectOptions = _dbContext.FAQs
                    .Where(x => x.Type.Equals(type))
                    .Select(strat =>
                        new SelectMenuOptionBuilder
                        {
                            Value = strat.Identifier,
                            Description = "-",
                            Label = strat.Label
                        })
                    .ToList();

                var selectMenu = new SelectMenuBuilder()
                    .WithCustomId($"manageFAQ_{type}")
                    .WithPlaceholder(BotMessage.QuestionSelect)
                    .WithOptions(selectOptions);

                var addButton = new ButtonBuilder()
                    .WithCustomId($"btnAdd_{type}")
                    .WithLabel("Add")
                    .WithStyle(ButtonStyle.Primary);

                var editButton = new ButtonBuilder()
                    .WithCustomId($"btnEdit_{identifier},{type}")
                    .WithLabel("Edit")
                    .WithDisabled(disabled)
                    .WithStyle(ButtonStyle.Success);

                var removeButton = new ButtonBuilder()
                    .WithCustomId($"btnRemove_{identifier},{type}")
                    .WithLabel("Remove")
                    .WithDisabled(disabled)
                    .WithStyle(ButtonStyle.Danger);

                var refreshButton = new ButtonBuilder()
                    .WithCustomId($"btnRefresh_{type}")
                    .WithLabel("Refresh")
                    .WithStyle(ButtonStyle.Secondary);

                var selectRow = new ActionRowBuilder()
                    .WithSelectMenu(selectMenu);

                var crudButtons = new ActionRowBuilder()
                    .WithButton(addButton)
                    .WithButton(editButton)
                    .WithButton(removeButton)
                    .WithButton(refreshButton);

                var messageComponent = new ComponentBuilder()
                    .WithRows(new List<ActionRowBuilder>
                    {
                        selectRow,
                        crudButtons
                    })
                    .Build();

                return messageComponent;
            }
        }

        [ModalInteraction("addFAQMdl_*,*", ignoreGroupNames: true)]
        public async Task HandleAddQuestion(string guid, string questionType, StratQuestionsModal modal)
        {
            try
            {
                if (_dbContext.FAQs.Any(x => x.Identifier == modal.Identifier && x.Type == questionType))
                    throw new BotException(BotMessage.QuestionAlreadyExists(modal.Identifier));

                var strat = new FAQ
                {
                    Label = modal.Label,
                    Identifier = modal.Identifier,
                    Description = modal.Description,
                    Content = modal.Content,
                    Type = questionType
                };

                _dbContext.FAQs.Add(strat);
                _dbContext.SaveChanges();

                await RespondAsync(BotMessage.QuestionAdded, ephemeral: true);
            }
            catch (BotException e)
            {
                await RespondAsync(e.Message);
            }
            catch (Exception e)
            {
                await RespondAsync(BotMessage.InternalError, ephemeral: true);
                Log.Error(e, e.Message);
            }
        }

        [ModalInteraction("editFAQMdl_*,*,*", ignoreGroupNames: true)]
        public async Task HandleEditQuestion(string guid, int questionId, string questionType, StratQuestionsModal modal)
        {
            try
            {
                if (_dbContext.FAQs.Any(x => x.Identifier == modal.Identifier && x.Type == questionType && x.Id != questionId))
                    throw new BotException(BotMessage.QuestionAlreadyExists(modal.Identifier));

                var strat = _dbContext.FAQs
                    .Where(x => x.Id == questionId)
                    .FirstOrDefault();

                strat.Content = modal.Content;
                strat.Description = modal.Description;
                strat.Identifier = modal.Identifier;
                strat.Label = modal.Label;

                _dbContext.FAQs.Update(strat);
                _dbContext.SaveChanges();

                await RespondAsync(BotMessage.QuestionEdited, ephemeral: true);
            }
            catch (BotException e)
            {
                await RespondAsync(e.Message);
            }
            catch (Exception e)
            {
                await RespondAsync(BotMessage.InternalError, ephemeral: true);
                Log.Error(e, e.Message);
            }
        }
    }
}