using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace GvG_Core_Bot.Main.Database.Models
{
    public class RoleCommandsInfo
    {
        [Required, Key] public int RoleCommandID { get; set; }
        public string CommandMove { get; set; }
        public string CommandDescription { get; set; }
        public string CommandArgumentsDescription { get; set; }

        [ForeignKey("RoleTableInfo")] public int RoleID { get; set; }
        public RoleTableInfo Role { get; set; }

        internal static void Seed(DbSet<RoleCommandsInfo> commands, DbSet<RoleTableInfo> roles)
        {
            commands.Add(new RoleCommandsInfo() { RoleCommandID = 1, });
        }
    }
}
