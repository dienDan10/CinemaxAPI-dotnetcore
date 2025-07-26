using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CinemaxAPI.Migrations
{
    /// <inheritdoc />
    public partial class addSeatType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "TicketPrice",
                table: "ShowTimes",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AddColumn<decimal>(
                name: "VipTicketPrice",
                table: "ShowTimes",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "SeatType",
                table: "Seats",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CheckedInBy",
                table: "Bookings",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_CheckedInBy",
                table: "Bookings",
                column: "CheckedInBy");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_AspNetUsers_CheckedInBy",
                table: "Bookings",
                column: "CheckedInBy",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_AspNetUsers_CheckedInBy",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_CheckedInBy",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "VipTicketPrice",
                table: "ShowTimes");

            migrationBuilder.DropColumn(
                name: "SeatType",
                table: "Seats");

            migrationBuilder.DropColumn(
                name: "CheckedInBy",
                table: "Bookings");

            migrationBuilder.AlterColumn<double>(
                name: "TicketPrice",
                table: "ShowTimes",
                type: "float",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");
        }
    }
}
