using System.Security.AccessControl;
using System.Text;
using System.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Rumbi.Data;
using Rumbi.Modals;
using Rumbi.Data.Models;
using Rumbi.Exceptions;
using Serilog;
using Rumbi.Utils;

namespace Rumbi.CommandsModules.Speedrunning
{
    [Group("faq", "Frequently asked questions about hat speedrunning")]
    public class FAQModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly RumbiContext _dbContext;

        public FAQModule(RumbiContext dbContext)
        {
            _dbContext = dbContext;
        }

        [SlashCommand("strats", "Learn about strats!")]
        public async Task ShowStratsFAQ()
        {
            if (_dbContext.Strats.Count() == 0)
            {
                await RespondAsync(BotMessage.QuestionsEmpty, ephemeral: true);
                return;
            }

            await DeferAsync(ephemeral: true);

            await FollowupAsync(
                text: BotMessage.QuestionWelcomeMessage,
                components: GetStratsMenuOptions(),
                ephemeral: true
            );
        }

        [ComponentInteraction("faqstrats", ignoreGroupNames: true)]
        public async Task StratsFAQSelection(string[] selectedQuestion)
        {
            var interactionContext = Context.Interaction as IComponentInteraction;

            var content = _dbContext.Strats
                .Where(x => x.Identifier == selectedQuestion[0])
                .Select(y => y.Content)
                .SingleOrDefault();

            await RespondAsync(content);
        }

        private MessageComponent GetStratsMenuOptions()
        {
            var selectOptions = _dbContext.Strats
                .Select(
                    strat =>
                        new SelectMenuOptionBuilder
                        {
                            Value = strat.Identifier,
                            Description = "-",
                            Label = strat.Label
                        }
                )
                .ToList();

            var selectMenu = new SelectMenuBuilder()
                .WithCustomId("faqstrats")
                .WithPlaceholder(BotMessage.QuestionSelect)
                .WithOptions(selectOptions);

            var messageComponent = new ComponentBuilder().WithSelectMenu(selectMenu).Build();

            return messageComponent;
        }

        [Group("strats-management", "Manage strats questions")]
        public class StratsManagementModule : InteractionModuleBase<SocketInteractionContext>
        {
            private readonly RumbiContext _dbContext;

            public StratsManagementModule(RumbiContext dbContext)
            {
                _dbContext = dbContext;
            }

            [SlashCommand("add", "Add a new strat question/answer")]
            public async Task AddStratQuestion()
            {
                var guid = Guid.NewGuid();
                await Context.Interaction.RespondWithModalAsync<StratQuestionsModal>(
                    $"addQuestionModal_{guid}",
                    null,
                    x => x.WithTitle(BotMessage.QuestionModalAdd)
                );
            }

            [SlashCommand("edit", "Edit a new strat question/answer")]
            public async Task EditStratQuestion(
                [Summary("identifier", "The unique identifier such as 'sdj'")] string identifier
            )
            {
                await DeferAsync();

                var strat = _dbContext.Strats
                    .Where(x => x.Identifier == identifier)
                    .FirstOrDefault();

                if (strat == null)
                    await FollowupAsync(BotMessage.QuestionNotFound(identifier));

                var guid = Guid.NewGuid();
                await Context.Interaction.RespondWithModalAsync<StratQuestionsModal>(
                    $"editQuestionModal_{guid}",
                    new StratQuestionsModal
                    {
                        Content = strat.Content,
                        Identifier = strat.Identifier,
                        Label = strat.Label,
                        Description = strat.Description
                    },
                    null,
                    x => x.WithTitle(BotMessage.QuestionModalEdit)
                );
            }

            [SlashCommand("remove", "Remove a new strat question/answer")]
            public async Task RemoveStratQuestion(
                [Summary("identifier", "The unique identifier such as 'sdj'")] string identifier
            )
            {
                await DeferAsync();

                var strat = _dbContext.Strats
                    .Where(x => x.Identifier == identifier)
                    .FirstOrDefault();

                if (strat == null)
                {
                    await FollowupAsync(BotMessage.QuestionNotFound(identifier));
                }

                _dbContext.Strats.Remove(strat);
                _dbContext.SaveChanges();

                await FollowupAsync(BotMessage.QuestionDeleted);
            }

            [ModalInteraction("addQuestionModal_*", ignoreGroupNames: true)]
            public async Task HandleAddQuestion(string guid, StratQuestionsModal modal)
            {
                try
                {
                    if (_dbContext.Strats.Any(x => x.Identifier == modal.Identifier))
                        throw new BotException(BotMessage.QuestionAlreadyExists(modal.Identifier));

                    var strat = new Strat
                    {
                        Label = modal.Label,
                        Identifier = modal.Identifier,
                        Description = modal.Description,
                        Content = modal.Content
                    };

                    _dbContext.Strats.Add(strat);
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

            [ModalInteraction("editQuestionModal_*", ignoreGroupNames: true)]
            public async Task HandleEditQuestion(string guid, StratQuestionsModal modal)
            {
                try
                {
                    if (_dbContext.Strats.Any(x => x.Identifier == modal.Identifier))
                        throw new BotException(BotMessage.QuestionAlreadyExists(modal.Identifier));

                    var strat = _dbContext.Strats
                        .Where(x => x.Id == modal.QuestionId)
                        .FirstOrDefault();

                    strat.Content = modal.Content;
                    strat.Description = modal.Description;
                    strat.Identifier = modal.Identifier;
                    strat.Label = modal.Label;

                    _dbContext.Strats.Update(strat);
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
}
