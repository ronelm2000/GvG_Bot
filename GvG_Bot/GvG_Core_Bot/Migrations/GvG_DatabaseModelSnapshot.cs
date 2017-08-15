using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using GvG_Core_Bot.Main.Database;

namespace GvG_Core_Bot.Migrations
{
    [DbContext(typeof(GvG_Database))]
    partial class GvG_DatabaseModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.1.1");

            modelBuilder.Entity("GvG_Core_Bot.Main.Database.Models.RoleCommandsInfo", b =>
                {
                    b.Property<int>("RoleCommandID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("CommandArgumentsDescription");

                    b.Property<string>("CommandDescription");

                    b.Property<string>("CommandMove");

                    b.Property<int>("RoleID");

                    b.HasKey("RoleCommandID");

                    b.HasIndex("RoleID");

                    b.ToTable("Commands");
                });

            modelBuilder.Entity("GvG_Core_Bot.Main.Database.Models.RoleTableInfo", b =>
                {
                    b.Property<int>("RoleID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("RoleClassName");

                    b.Property<string>("RoleDescription");

                    b.Property<string>("RoleName");

                    b.HasKey("RoleID");

                    b.ToTable("RoleInfos");
                });

            modelBuilder.Entity("GvG_Core_Bot.Main.Database.Models.RoleCommandsInfo", b =>
                {
                    b.HasOne("GvG_Core_Bot.Main.Database.Models.RoleTableInfo", "Role")
                        .WithMany("RoleCommands")
                        .HasForeignKey("RoleID")
                        .OnDelete(DeleteBehavior.Cascade);
                });
        }
    }
}
