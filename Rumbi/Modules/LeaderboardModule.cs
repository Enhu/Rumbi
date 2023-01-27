using Discord.Interactions;
using Rumbi.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rumbi.Modules
{
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

        }

        [SlashCommand("close-poll", "Create a new leadearboard announcement")]
        public async Task CloseLeaderboardPoll()
        {

        }

        [SlashCommand("announcement", "Create a new leadearboard announcement")]
        public async Task CreateLeaderboardAnnouncement()
        {

        }
    }
}
