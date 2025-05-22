using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace eKIBRA.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class ChangeFlashcardIndexNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "QuestionText",
                table: "Flashcards",
                newName: "IX_Flashcards_UserId_DeckId_Question");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "IX_Flashcards_UserId_DeckId_Question",
                table: "Flashcards",
                newName: "QuestionText");
        }
    }
}
