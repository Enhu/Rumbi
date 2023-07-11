using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rumbi.Data.Migrations
{
    /// <inheritdoc />
    public partial class StratValueUniqueConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddUniqueConstraint(
                name: "AK_Strats_Value",
                table: "Strats",
                column: "Value");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropUniqueConstraint(
                name: "AK_Strats_Value",
                table: "Strats");
        }
    }
}
