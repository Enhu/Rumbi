using Discord.Interactions;
using Discord.WebSocket;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rumbi.Data.Config;

namespace Rumbi.PreConditions
{
    internal class RequireModerator : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
        {
            var user = context.Interaction.User as SocketGuildUser;

            var config = new AppConfig();

            var hasPermission = user.Roles.Any(x => x.Id == config.RoleConfig.Verifier || x.Id == config.RoleConfig.SRDCMod || x.Id == config.RoleConfig.Admin);

            if (hasPermission)
                return Task.FromResult(PreconditionResult.FromSuccess());
            else
                return Task.FromResult(PreconditionResult.FromError("You don't have permissions to do this."));
        }
    }
}