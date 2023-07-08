using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rumbi.Data.Migrations
{
    /// <inheritdoc />
    public partial class StratChangeValueToIdentifier : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropUniqueConstraint(
                name: "AK_Strats_Value",
                table: "Strats");

            migrationBuilder.RenameColumn(
                name: "Value",
                table: "Strats",
                newName: "Identifier");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Strats_Identifier",
                table: "Strats",
                column: "Identifier");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropUniqueConstraint(
                name: "AK_Strats_Identifier",
                table: "Strats");

            migrationBuilder.RenameColumn(
                name: "Identifier",
                table: "Strats",
                newName: "Value");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Strats_Value",
                table: "Strats",
                column: "Value");
        }
    }
}
