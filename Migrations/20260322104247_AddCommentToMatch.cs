using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TableTennisHistoric.Migrations
{
    /// <inheritdoc />
    public partial class AddCommentToMatch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Comment",
                table: "TableTennisMatch",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Comment",
                table: "TableTennisMatch");
        }
    }
}
