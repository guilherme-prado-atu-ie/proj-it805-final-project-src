using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace eKIBRA.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixStudyEntityWithDeck : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StudySessions_DeckId",
                table: "StudySessions");

            migrationBuilder.CreateIndex(
                name: "IX_StudySessions_DeckId",
                table: "StudySessions",
                column: "DeckId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StudySessions_DeckId",
                table: "StudySessions");

            migrationBuilder.CreateIndex(
                name: "IX_StudySessions_DeckId",
                table: "StudySessions",
                column: "DeckId",
                unique: true);
        }
    }
}
