using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Rumbi.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Config");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GuildUsers",
                table: "GuildUsers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GuildRoles",
                table: "GuildRoles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GuildChannels",
                table: "GuildChannels");

            migrationBuilder.RenameTable(
                name: "GuildUsers",
                newName: "Users");

            migrationBuilder.RenameTable(
                name: "GuildRoles",
                newName: "Roles");

            migrationBuilder.RenameTable(
                name: "GuildChannels",
                newName: "Channels");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Users",
                table: "Users",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Roles",
                table: "Roles",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Channels",
                table: "Channels",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Users",
                table: "Users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Roles",
                table: "Roles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Channels",
                table: "Channels");

            migrationBuilder.RenameTable(
                name: "Users",
                newName: "GuildUsers");

            migrationBuilder.RenameTable(
                name: "Roles",
                newName: "GuildRoles");

            migrationBuilder.RenameTable(
                name: "Channels",
                newName: "GuildChannels");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GuildUsers",
                table: "GuildUsers",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GuildRoles",
                table: "GuildRoles",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GuildChannels",
                table: "GuildChannels",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Config",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TwitchAccessToken = table.Column<int>(type: "integer", nullable: false),
                    TwitchTokenExpirationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Config", x => x.Id);
                });
        }
    }
}
