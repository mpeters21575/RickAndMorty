using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RickAndMorty.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEpisodes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Episodes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    EpisodeCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    AirDate = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Episodes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Episodes_CreatedAt",
                table: "Episodes",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Episodes_EpisodeCode",
                table: "Episodes",
                column: "EpisodeCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Episodes");
        }
    }
}
