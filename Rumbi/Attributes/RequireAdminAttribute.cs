using Discord.Interactions;
using Discord.WebSocket;
using Discord;
using Rumbi.Data.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rumbi.Attributes
{
    internal class RequireAdmin : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
        {
            var user = context.Interaction.User as SocketGuildUser;

            var config = new AppConfig();

            var hasAdmin = user.Roles.Any(x => x.Id == config.RoleConfig.Admin);

            if (hasAdmin)
                return Task.FromResult(PreconditionResult.FromSuccess());
            else
                return Task.FromResult(PreconditionResult.FromError("You don't have permissions to do this."));
        }
    }
}