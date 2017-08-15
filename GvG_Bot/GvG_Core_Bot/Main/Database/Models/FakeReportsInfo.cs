using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace GvG_Core_Bot.Main.Database.Models
{
    class FakeReportsInfo
    {
        [Key] public int FakeReportID { get; set; }
        public ulong UserID { get; set; }
        public string FakeReportOutput { get; set; }

        [ForeignKey("RoleCommandsInfo")] public int RoleCommandID { get; set; }
        RoleCommandsInfo RoleCommand { get; set; }
    }
}
