using Discord.Interactions;
using Microsoft.EntityFrameworkCore;
using Rumbi.Data;
using Rumbi.Data.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rumbi.Modules 
{
    [RequireUserPermission(Discord.GuildPermission.ManageRoles)]
    public class MemeModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly RumbiContext _dbContext;

        public MemeModule(RumbiContext context)
        {
            _dbContext = context;
        }

        [SlashCommand("addmeme", "Add a new meme.")]
        public async Task AddMeme([Summary(description: "The trigger for the meme.")] string trigger, [Summary(description: "The content of the meme.")] string content)
        {
            try
            {
                var memeExists = _dbContext.Memes.Any(x => x.Trigger == trigger);

                if (memeExists)
                {
                    await RespondAsync("The meme already exists, modify it or use another trigger word.", ephemeral: true);
                    return;
                }

                var meme = new Meme
                {
                    Content = content,
                    Trigger = trigger,
                };

                _dbContext.Add(meme);
                _dbContext.SaveChanges();

                await RespondAsync("Meme added.", ephemeral: true);
            }
            catch (Exception e)
            {
                Log.Error(e, e.Message, e.InnerException);
            }
        }

        [SlashCommand("removememe", "Remove a meme.")]
        public async Task AddMeme([Summary(description: "The trigger for the meme.")] string trigger)
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

        [SlashCommand("modifymeme", "Modify a meme.")]
        public async Task ModifyMeme([Summary(description: "The trigger for the meme.")] string trigger, [Summary(description: "The content of the meme.")] string content)
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

    }
}
