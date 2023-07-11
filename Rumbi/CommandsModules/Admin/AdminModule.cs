using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.Interactions;
using Rumbi.Attributes;

namespace Rumbi.CommandsModules.Admin
{
    [RequireAdmin]
    public class AdminModule : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("prune", "Prune the last messages.")]
        public async Task PruneMessages(
            [Summary("number", "The number of messages you want to prune.")]
            [MinValue(1)]
            [MaxValue(100)]
                int messagesNumber
        )
        {
            throw new NotImplementedException();
        }
    }
}