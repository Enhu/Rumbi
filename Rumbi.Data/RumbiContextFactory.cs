using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rumbi.Data
{
    public class RumbiContextFactory : IDesignTimeDbContextFactory<RumbiContext>
    {
        public RumbiContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<RumbiContext>()
                .UseNpgsql("Server=127.0.0.1;Port=5432;Databse=RumbiDB;User Id=postgres;Password=admin");

            return new RumbiContext(optionsBuilder.Options);
        }
    }
}
