using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RickAndMorty.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Characters",
                table: "Episodes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Url",
                table: "Episodes",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Characters",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Characters",
                table: "Episodes");

            migrationBuilder.DropColumn(
                name: "Url",
                table: "Episodes");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Characters");
        }
    }
}
