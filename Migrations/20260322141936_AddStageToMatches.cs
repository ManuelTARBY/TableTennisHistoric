using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TableTennisHistoric.Migrations
{
    /// <inheritdoc />
    public partial class AddStageToMatches : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StageId",
                table: "TableTennisMatch",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Stage",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stage", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_TableTennisMatch_StageId",
                table: "TableTennisMatch",
                column: "StageId");

            migrationBuilder.AddForeignKey(
                name: "FK_TableTennisMatch_Stage_StageId",
                table: "TableTennisMatch",
                column: "StageId",
                principalTable: "Stage",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TableTennisMatch_Stage_StageId",
                table: "TableTennisMatch");

            migrationBuilder.DropTable(
                name: "Stage");

            migrationBuilder.DropIndex(
                name: "IX_TableTennisMatch_StageId",
                table: "TableTennisMatch");

            migrationBuilder.DropColumn(
                name: "StageId",
                table: "TableTennisMatch");
        }
    }
}
