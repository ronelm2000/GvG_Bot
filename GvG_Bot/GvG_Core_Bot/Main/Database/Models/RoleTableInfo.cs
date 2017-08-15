using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Resources;
using Microsoft.EntityFrameworkCore;

namespace GvG_Core_Bot.Main.Database.Models
{
    public class RoleTableInfo
    {
        [Required, Key] public int RoleID { get; set; }
        public string RoleClassName { get; set; }
        public string RoleName { get; set; }
        public string RoleDescription { get; set; }

        public List<RoleCommandsInfo> RoleCommands { get; set; }

        internal static void Seed(DbSet<RoleTableInfo> roleInfos)
        {
            var desc = RoleDescriptions.ResourceManager;
            roleInfos.AddRange(
                new RoleTableInfo() { RoleID = 1, RoleClassName = "Kotori", RoleName = "Kotori Kanbe", RoleDescription = desc.GetString("KotoriDesc") },
                new RoleTableInfo() { RoleID = 2, RoleClassName = "NormalCivilian", RoleName = "Normal Civilian", RoleDescription = desc.GetString("NormalCivilianDesc") },
                new RoleTableInfo() { RoleID = 3, RoleClassName = "Kagari", RoleName = "Kagari (or the Key)", RoleDescription = desc.GetString("KagariDesc") },
                new RoleTableInfo() { RoleID = 4, RoleClassName = "Kotarou", RoleName = "Kotarou Tennouji", RoleDescription = desc.GetString("KotarouDesc") }
                );
            
        }
    }
}
