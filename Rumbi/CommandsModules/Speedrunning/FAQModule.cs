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
    }
}