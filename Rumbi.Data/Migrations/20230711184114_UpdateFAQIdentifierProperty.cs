using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rumbi.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateFAQIdentifierProperty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropUniqueConstraint(
                name: "AK_FAQs_Identifier",
                table: "FAQs");

            migrationBuilder.CreateIndex(
                name: "IX_FAQs_Identifier",
                table: "FAQs",
                column: "Identifier",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FAQs_Identifier",
                table: "FAQs");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_FAQs_Identifier",
                table: "FAQs",
                column: "Identifier");
        }
    }
}
