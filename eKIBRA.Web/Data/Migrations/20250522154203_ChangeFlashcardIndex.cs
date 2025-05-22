using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace eKIBRA.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class ChangeFlashcardIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Flashcards_UserId",
                table: "Flashcards");

            migrationBuilder.DropIndex(
                name: "QuestionText",
                table: "Flashcards");

            migrationBuilder.DropIndex(
                name: "DeckTitle",
                table: "Decks");

            migrationBuilder.DropIndex(
                name: "IX_Decks_UserId",
                table: "Decks");

            migrationBuilder.CreateIndex(
                name: "QuestionText",
                table: "Flashcards",
                columns: new[] { "UserId", "DeckId", "Question" },
                unique: true,
                filter: "[Question] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Decks_UserId_Title",
                table: "Decks",
                columns: new[] { "UserId", "Title" },
                unique: true,
                filter: "[Title] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "QuestionText",
                table: "Flashcards");

            migrationBuilder.DropIndex(
                name: "IX_Decks_UserId_Title",
                table: "Decks");

            migrationBuilder.CreateIndex(
                name: "IX_Flashcards_UserId",
                table: "Flashcards",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "QuestionText",
                table: "Flashcards",
                column: "Question",
                unique: true,
                filter: "[Question] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "DeckTitle",
                table: "Decks",
                columns: new[] { "Title", "UserId" },
                unique: true,
                filter: "[Title] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Decks_UserId",
                table: "Decks",
                column: "UserId");
        }
    }
}
