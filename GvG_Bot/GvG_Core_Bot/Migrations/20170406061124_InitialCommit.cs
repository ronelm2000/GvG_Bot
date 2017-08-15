using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GvG_Core_Bot.Migrations
{
    public partial class InitialCommit : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RoleInfos",
                columns: table => new
                {
                    RoleID = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RoleClassName = table.Column<string>(nullable: true),
                    RoleDescription = table.Column<string>(nullable: true),
                    RoleName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleInfos", x => x.RoleID);
                });

            migrationBuilder.CreateTable(
                name: "Commands",
                columns: table => new
                {
                    RoleCommandID = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CommandArgumentsDescription = table.Column<string>(nullable: true),
                    CommandDescription = table.Column<string>(nullable: true),
                    CommandMove = table.Column<string>(nullable: true),
                    RoleID = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Commands", x => x.RoleCommandID);
                    table.ForeignKey(
                        name: "FK_Commands_RoleInfos_RoleID",
                        column: x => x.RoleID,
                        principalTable: "RoleInfos",
                        principalColumn: "RoleID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Commands_RoleID",
                table: "Commands",
                column: "RoleID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Commands");

            migrationBuilder.DropTable(
                name: "RoleInfos");
        }
    }
}
