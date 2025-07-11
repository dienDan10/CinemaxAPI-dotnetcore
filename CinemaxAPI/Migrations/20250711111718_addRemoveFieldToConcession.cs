using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CinemaxAPI.Migrations
{
    /// <inheritdoc />
    public partial class addRemoveFieldToConcession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsRemoved",
                table: "Concessions",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsRemoved",
                table: "Concessions");
        }
    }
}
