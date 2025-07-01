using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CinemaxAPI.Migrations
{
    /// <inheritdoc />
    public partial class ChangeTheaterTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClosingTime",
                table: "Theaters");

            migrationBuilder.DropColumn(
                name: "OpeningTime",
                table: "Theaters");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Theaters",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Theaters");

            migrationBuilder.AddColumn<TimeOnly>(
                name: "ClosingTime",
                table: "Theaters",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "OpeningTime",
                table: "Theaters",
                type: "time",
                nullable: true);
        }
    }
}
