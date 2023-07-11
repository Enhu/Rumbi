using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.Interactions;

namespace Rumbi.CommandsModules.Admin
{
    public class AdminModule : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("prune", "Prune the last messages.")]
        public async Task PruneMessages(
            [Summary("number", "The number of messages you want to prune.")]
            [MinValue(1)]
            [MaxValue(100)]
                int messagesNumber
        ) { }
    }
}
