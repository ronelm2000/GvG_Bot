using GvG_Core_Bot.Main.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GvG_Core_Bot.Main.Database
{
    public class GvG_DatabaseFactory : IDbContextFactory<GvG_Database>
    {
        public GvG_Database Create(DbContextFactoryOptions options)
        {
            var builder = new DbContextOptionsBuilder<GvG_Database>();
            builder.UseSqlite("Filename=GvG.db");
            return new GvG_Database(builder.Options);
        }
    }

    public class GvG_Database : DbContext
    {
        // note I need to hard-code the info to the database so that it can be accepted as method thingys
        public GvG_Database (DbContextOptions<GvG_Database> options) : base(options)
        {

        }

        public DbSet<RoleCommandsInfo> Commands { get; set; }
        public DbSet<RoleTableInfo> RoleInfos { get; set; }

        public void EnsureSeedData ()
        {
            if (Database.GetPendingMigrations().Count() > 0) Database.Migrate();
            if (RoleInfos.Count() == 0)
            {
                RoleCommandsInfo.Seed(Commands, RoleInfos);
                RoleTableInfo.Seed(RoleInfos);
            }
        }
    }
}
