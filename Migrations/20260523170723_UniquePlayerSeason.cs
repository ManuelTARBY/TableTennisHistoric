using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TableTennisHistoric.Migrations
{
    /// <inheritdoc />
    public partial class UniquePlayerSeason : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
    name: "IX_PlayerSeason_PlayerId_SeasonId",
    table: "PlayerSeason",
    columns: new[] { "PlayerId", "SeasonId" },
    unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
    name: "IX_PlayerSeason_PlayerId_SeasonId",
    table: "PlayerSeason");
        }
    }
}
