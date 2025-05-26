using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace eKIBRA.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class ResizeFlashcardNVarcharFieldsToFitIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Question",
                table: "Flashcards",
                type: "nvarchar(700)",
                maxLength: 700,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000);

            migrationBuilder.AlterColumn<string>(
                name: "Answer",
                table: "Flashcards",
                type: "nvarchar(700)",
                maxLength: 700,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Question",
                table: "Flashcards",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(700)",
                oldMaxLength: 700);

            migrationBuilder.AlterColumn<string>(
                name: "Answer",
                table: "Flashcards",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(700)",
                oldMaxLength: 700);
        }
    }
}
