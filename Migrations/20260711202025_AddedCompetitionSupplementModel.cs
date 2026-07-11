using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TableTennisHistoric.Migrations
{
    /// <inheritdoc />
    public partial class AddedCompetitionSupplementModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CompetitionSupplementId",
                table: "TableTennisMatch",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CompetitionSupplement",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompetitionSupplement", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_TableTennisMatch_CompetitionSupplementId",
                table: "TableTennisMatch",
                column: "CompetitionSupplementId");

            migrationBuilder.AddForeignKey(
                name: "FK_TableTennisMatch_CompetitionSupplement_CompetitionSupplement~",
                table: "TableTennisMatch",
                column: "CompetitionSupplementId",
                principalTable: "CompetitionSupplement",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TableTennisMatch_CompetitionSupplement_CompetitionSupplement~",
                table: "TableTennisMatch");

            migrationBuilder.DropTable(
                name: "CompetitionSupplement");

            migrationBuilder.DropIndex(
                name: "IX_TableTennisMatch_CompetitionSupplementId",
                table: "TableTennisMatch");

            migrationBuilder.DropColumn(
                name: "CompetitionSupplementId",
                table: "TableTennisMatch");
        }
    }
}
