using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace eKIBRA.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class ChangeDeckIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Flashcards_Decks_DeckId",
                table: "Flashcards");

            migrationBuilder.DropIndex(
                name: "DeckTitle",
                table: "Decks");

            migrationBuilder.CreateIndex(
                name: "DeckTitle",
                table: "Decks",
                columns: new[] { "Title", "UserId" },
                unique: true,
                filter: "[Title] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Flashcards_Decks_DeckId",
                table: "Flashcards",
                column: "DeckId",
                principalTable: "Decks",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Flashcards_Decks_DeckId",
                table: "Flashcards");

            migrationBuilder.DropIndex(
                name: "DeckTitle",
                table: "Decks");

            migrationBuilder.CreateIndex(
                name: "DeckTitle",
                table: "Decks",
                column: "Title",
                unique: true,
                filter: "[Title] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Flashcards_Decks_DeckId",
                table: "Flashcards",
                column: "DeckId",
                principalTable: "Decks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
