using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TableTennisHistoric.Migrations
{
    /// <inheritdoc />
    public partial class UniqueSeasonCompetition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Supprimer d'abord la clé étrangère
            migrationBuilder.DropForeignKey(
                name: "FK_CompetitionCoefficient_Season_SeasonId",
                table: "CompetitionCoefficient");

            // 2. Supprimer l'ancien index simple
            migrationBuilder.DropIndex(
                name: "IX_CompetitionCoefficient_SeasonId",
                table: "CompetitionCoefficient");

            // 3. Créer le nouvel index unique composé
            migrationBuilder.CreateIndex(
                name: "IX_CompetitionCoefficient_SeasonId_CompetitionId",
                table: "CompetitionCoefficient",
                columns: new[] { "SeasonId", "CompetitionId" },
                unique: true);

            // 4. Recréer la clé étrangère sur le nouvel index
            migrationBuilder.AddForeignKey(
                name: "FK_CompetitionCoefficient_Season_SeasonId",
                table: "CompetitionCoefficient",
                column: "SeasonId",
                principalTable: "Season",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
        name: "FK_CompetitionCoefficient_Season_SeasonId",
        table: "CompetitionCoefficient");

            migrationBuilder.DropIndex(
                name: "IX_CompetitionCoefficient_SeasonId_CompetitionId",
                table: "CompetitionCoefficient");

            migrationBuilder.CreateIndex(
                name: "IX_CompetitionCoefficient_SeasonId",
                table: "CompetitionCoefficient",
                column: "SeasonId");

            migrationBuilder.AddForeignKey(
                name: "FK_CompetitionCoefficient_Season_SeasonId",
                table: "CompetitionCoefficient",
                column: "SeasonId",
                principalTable: "Season",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
