using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace eKIBRA.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class StudyEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StudySessions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    DeckId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Modified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifierUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    Version = table.Column<Guid>(type: "uniqueidentifier", maxLength: 450, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudySessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudySessions_AspNetUsers_ModifierUserId",
                        column: x => x.ModifierUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StudySessions_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StudySessions_Decks_DeckId",
                        column: x => x.DeckId,
                        principalTable: "Decks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FlashcardsProgress",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    StudySessionId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    DeckId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    FlashcardId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false),
                    Reveals = table.Column<int>(type: "int", nullable: false),
                    RevealAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Remembers = table.Column<int>(type: "int", nullable: false),
                    RememberAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Forgets = table.Column<int>(type: "int", nullable: false),
                    ForgetAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Level = table.Column<int>(type: "int", nullable: false),
                    RevealsAcrossSessions = table.Column<int>(type: "int", nullable: false),
                    RemembersAcrossSessions = table.Column<int>(type: "int", nullable: false),
                    ForgetsAcrossSessions = table.Column<int>(type: "int", nullable: false),
                    SpacedRepetitionInterval = table.Column<int>(type: "int", nullable: false),
                    NextSpacedRepetitionInterval = table.Column<int>(type: "int", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Modified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifierUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Version = table.Column<Guid>(type: "uniqueidentifier", maxLength: 450, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlashcardsProgress", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FlashcardsProgress_AspNetUsers_ModifierUserId",
                        column: x => x.ModifierUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FlashcardsProgress_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FlashcardsProgress_Decks_DeckId",
                        column: x => x.DeckId,
                        principalTable: "Decks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FlashcardsProgress_Flashcards_FlashcardId",
                        column: x => x.FlashcardId,
                        principalTable: "Flashcards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FlashcardsProgress_StudySessions_StudySessionId",
                        column: x => x.StudySessionId,
                        principalTable: "StudySessions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_FlashcardsProgress_DeckId",
                table: "FlashcardsProgress",
                column: "DeckId");

            migrationBuilder.CreateIndex(
                name: "IX_FlashcardsProgress_FlashcardId",
                table: "FlashcardsProgress",
                column: "FlashcardId");

            migrationBuilder.CreateIndex(
                name: "IX_FlashcardsProgress_ModifierUserId",
                table: "FlashcardsProgress",
                column: "ModifierUserId");

            migrationBuilder.CreateIndex(
                name: "IX_FlashcardsProgress_StudySessionId",
                table: "FlashcardsProgress",
                column: "StudySessionId");

            migrationBuilder.CreateIndex(
                name: "IX_FlashcardsProgress_UserId_DeckId_StudySessionId_Sequence_FlashcardId",
                table: "FlashcardsProgress",
                columns: new[] { "UserId", "DeckId", "StudySessionId", "Sequence", "FlashcardId" });

            migrationBuilder.CreateIndex(
                name: "IX_StudySessions_DeckId",
                table: "StudySessions",
                column: "DeckId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudySessions_ModifierUserId",
                table: "StudySessions",
                column: "ModifierUserId");

            migrationBuilder.CreateIndex(
                name: "IX_StudySessions_UserId_DeckId_Status",
                table: "StudySessions",
                columns: new[] { "UserId", "DeckId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FlashcardsProgress");

            migrationBuilder.DropTable(
                name: "StudySessions");
        }
    }
}
