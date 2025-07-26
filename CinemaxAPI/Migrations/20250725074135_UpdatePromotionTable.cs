using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CinemaxAPI.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePromotionTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Status",
                table: "Promotions",
                newName: "IsActive");

            migrationBuilder.AddColumn<int>(
                name: "UsedQuantity",
                table: "Promotions",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UsedQuantity",
                table: "Promotions");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "Promotions",
                newName: "Status");
        }
    }
}
